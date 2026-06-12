using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Users.Exceptions;

public sealed class InvalidBioException : DomainException
{
    public InvalidBioException(string reason)
        : base($"Bio is invalid: {reason}") { }
}