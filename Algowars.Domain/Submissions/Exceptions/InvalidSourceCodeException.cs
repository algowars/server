using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Submissions.Exceptions;

public sealed class InvalidSourceCodeException : DomainException
{
    public InvalidSourceCodeException(string reason)
        : base($"Source code is invalid: {reason}") { }
}
