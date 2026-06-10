using Algowars.Domain.SeedWork;

namespace Algowars.Domain.User.Exceptions;

public sealed class InvalidUsernameException(string reason) : DomainException($"Username is invalid: {reason}")
{
}