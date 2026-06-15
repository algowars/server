using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Exceptions;

public sealed class UsernameCooldownException(DateTime lastChangedAt)
    : DomainException($"Username can only be changed once every 30 days. Last changed at: {lastChangedAt}.")
{
}
