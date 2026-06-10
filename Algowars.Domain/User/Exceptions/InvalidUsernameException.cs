using Algowars.Domain.SeedWork;

namespace Algowars.Domain.User.Exceptions;

public sealed class InvalidUsernameException : DomainException
{
    public InvalidUsernameException(string reason)
        : base($"Username is invalid: {reason}") { }
}