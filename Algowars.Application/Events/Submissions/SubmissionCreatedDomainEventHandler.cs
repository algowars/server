using Algowars.Application.Messaging;
using Algowars.Application.Messaging.Messages;
using Algowars.Domain.Submissions.Events;
using MediatR;

namespace Algowars.Application.Events.Submissions;

internal sealed class SubmissionCreatedDomainEventHandler(IMessagePublisher messagePublisher)
    : INotificationHandler<DomainEventNotification<SubmissionCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<SubmissionCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await messagePublisher.PublishAsync(
            new SubmissionCreatedMessage(notification.DomainEvent.SubmissionId),
            cancellationToken);
    }
}
