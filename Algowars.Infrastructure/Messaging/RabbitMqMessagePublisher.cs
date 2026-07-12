using System.Text;
using System.Text.Json;
using Algowars.Application.Messaging;
using RabbitMQ.Client;

namespace Algowars.Infrastructure.Messaging;

internal sealed class RabbitMqMessagePublisher(IConnection connection) : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class, IMessage
    {
        using var channel = connection.CreateModel();
        string queueName = QueueNames.ForType<T>();
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
        return Task.CompletedTask;
    }
}
