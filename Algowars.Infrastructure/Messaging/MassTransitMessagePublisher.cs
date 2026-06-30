using Algowars.Application.Messaging;
using MassTransit;

namespace Algowars.Infrastructure.Messaging;

internal sealed class MassTransitMessagePublisher(IPublishEndpoint publishEndpoint) : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class =>
        publishEndpoint.Publish(message, cancellationToken);
}
