using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Events;

public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;
