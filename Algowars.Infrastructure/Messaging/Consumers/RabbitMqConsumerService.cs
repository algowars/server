using System.Text;
using System.Text.Json;
using Algowars.Application.Configuration;
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
    MessageBusOptions options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumerService> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int consumerConcurrency = Math.Max(1, options.RabbitMQ.ConsumerConcurrency);
        var channels = new List<IModel>();

        SubscribeSubmissionMessages<SubmissionCreatedMessage>(
            channels,
            m => m.SubmissionId,
            consumerConcurrency,
            stoppingToken);

        SubscribeSubmissionMessages<SubmissionJobContinuationMessage>(
            channels,
            m => m.SubmissionId,
            consumerConcurrency,
            stoppingToken);

        stoppingToken.Register(() =>
        {
            foreach (var channel in channels)
            {
                try
                {
                    channel.Close();
                }
                catch
                {
                }

                channel.Dispose();
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Declares the queue for <typeparamref name="TMessage"/> and wires up async consumers that
    /// dispatch to <see cref="SubmissionJobProcessorService.RunForSubmissionAsync"/> for the
    /// submission id extracted from the message. The message is acknowledged only after processing
    /// succeeds; on failure it is nacked and requeued so it is not lost.
    /// </summary>
    private void SubscribeSubmissionMessages<TMessage>(
        ICollection<IModel> channels,
        Func<TMessage, Guid> getSubmissionId,
        int consumerConcurrency,
        CancellationToken stoppingToken)
        where TMessage : IMessage
    {
        string queueName = QueueNames.ForType<TMessage>();

        for (int i = 0; i < consumerConcurrency; i++)
        {
            IModel channel = connection.CreateModel();
            channels.Add(channel);

            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            // Limit to 1 unacknowledged message per consumer so each consumer
            // processes one message at a time; overall parallelism is controlled
            // by consumerConcurrency.
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            AsyncEventingBasicConsumer consumer = new(channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    string body = Encoding.UTF8.GetString(ea.Body.Span);
                    TMessage? message = JsonSerializer.Deserialize<TMessage>(body);
                    if (message is not null)
                    {
                        Guid submissionId = getSubmissionId(message);
                        LogSubmissionReceived(typeof(TMessage).Name, submissionId);
                        await ProcessAsync(submissionId, stoppingToken);
                    }

                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    LogProcessingError(ex);
                    // Requeue so a transient failure does not permanently drop the message.
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            channel.BasicConsume(queueName, autoAck: false, consumer);
        }
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
