using ApplicationCore.Interfaces.Messaging;
using MassTransit;

namespace Infrastructure.Messaging;

internal sealed class MassTransitMessagePublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class =>
        publishEndpoint.Publish(message, cancellationToken);
}