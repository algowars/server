using System.Text;
using System.Text.Json;
using Algowars.Application.Messaging.Messages;
using Algowars.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Algowars.Infrastructure.Messaging.Consumers;

internal sealed partial class RabbitMqConsumerService(
    IConnection connection,
    ILogger<RabbitMqConsumerService> logger
) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = connection.CreateModel();
        string queueName = QueueNames.ForType<SubmissionCreatedMessage>();
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, ea) =>
        {
            try
            {
                string body = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<SubmissionCreatedMessage>(body);
                if (message is not null)
                    LogSubmissionReceived(message.SubmissionId);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                LogProcessingError(ex);
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queueName, autoAck: false, consumer);

        stoppingToken.Register(() =>
        {
            channel.Close();
            channel.Dispose();
        });

        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Received SubmissionCreatedMessage for submission {SubmissionId}")]
    private partial void LogSubmissionReceived(Guid SubmissionId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Error processing message from RabbitMQ")]
    private partial void LogProcessingError(Exception ex);
}
