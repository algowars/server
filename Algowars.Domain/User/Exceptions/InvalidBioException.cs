using Algowars.Domain.SeedWork;

namespace Algowars.Domain.User.Exceptions;

public sealed class InvalidBioException : DomainException
{
    public InvalidBioException(string reason)
        : base($"Bio is invalid: {reason}") { }
}