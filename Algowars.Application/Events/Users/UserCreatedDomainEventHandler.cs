using Algowars.Domain.Authorization.Rbac;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Events;
using MediatR;

namespace Algowars.Application.Events.Users;

internal sealed class UserCreatedDomainEventHandler(IUserWriteRepository userWriteRepository)
    : INotificationHandler<DomainEventNotification<UserCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await userWriteRepository.AddToGroupAsync(
            notification.DomainEvent.UserId,
            WellKnownAuthorization.DefaultUserGroup,
            cancellationToken);
    }
}
