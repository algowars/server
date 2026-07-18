using System.Text;
using System.Text.Json;
using Algowars.Application.Messaging;
using Algowars.Application.Messaging.Messages;
using Algowars.Infrastructure.Jobs.Submissions;
using Algowars.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Algowars.Infrastructure.Messaging.Consumers;

internal sealed partial class RabbitMqConsumerService(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumerService> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = connection.CreateModel();

        SubscribeSubmissionMessages<SubmissionCreatedMessage>(
            channel, m => m.SubmissionId, stoppingToken);

        SubscribeSubmissionMessages<SubmissionJobContinuationMessage>(
            channel, m => m.SubmissionId, stoppingToken);

        stoppingToken.Register(() =>
        {
            channel.Close();
            channel.Dispose();
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Declares the queue for <typeparamref name="TMessage"/> and wires up a consumer that
    /// dispatches to <see cref="SubmissionJobProcessorService.RunForSubmissionAsync"/> for the
    /// submission id extracted from the message. Used for both the initial "submission created"
    /// queue and the "job continuation" queue published after each pipeline step.
    /// </summary>
    private void SubscribeSubmissionMessages<TMessage>(
        IModel channel,
        Func<TMessage, Guid> getSubmissionId,
        CancellationToken stoppingToken)
        where TMessage : IMessage
    {
        string queueName = QueueNames.ForType<TMessage>();
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, ea) =>
        {
            try
            {
                string body = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<TMessage>(body);
                if (message is not null)
                {
                    Guid submissionId = getSubmissionId(message);
                    LogSubmissionReceived(typeof(TMessage).Name, submissionId);
                    // Fire-and-forget on the thread pool; ack after dispatching.
                    _ = ProcessAsync(submissionId, stoppingToken);
                }
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                LogProcessingError(ex);
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queueName, autoAck: false, consumer);
    }

    private async Task ProcessAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<SubmissionJobProcessorService>();
            await processor.RunForSubmissionAsync(submissionId, cancellationToken);
        }
        catch (Exception ex)
        {
            LogProcessingError(ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Received {MessageType} for submission {SubmissionId}")]
    private partial void LogSubmissionReceived(string messageType, Guid submissionId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Error processing message from RabbitMQ")]
    private partial void LogProcessingError(Exception ex);
}