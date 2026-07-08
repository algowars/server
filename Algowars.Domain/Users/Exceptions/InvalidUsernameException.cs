using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Exceptions;

public sealed class InvalidUsernameException(string reason) : DomainException($"Username is invalid: {reason}")
{
}