using System.Text.Json;
using Algowars.Application.Messaging;
using Azure.Messaging.ServiceBus;

namespace Algowars.Infrastructure.Messaging;

internal sealed class AzureServiceBusMessagePublisher(ServiceBusClient client) : IMessagePublisher
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class, IMessage
    {
        await using var sender = client.CreateSender(QueueNames.ForType<T>());
        await sender.SendMessageAsync(
            new ServiceBusMessage(JsonSerializer.Serialize(message)),
            cancellationToken);
    }
}