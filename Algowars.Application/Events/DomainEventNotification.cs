using Algowars.Domain.SeedWork;
using MediatR;

namespace Algowars.Application.Events;

public sealed record DomainEventNotification<T>(T DomainEvent) : INotification
    where T : IDomainEvent;
