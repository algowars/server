using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class InvalidSlugException : DomainException
{
    public InvalidSlugException(string reason)
        : base($"Slug is invalid: {reason}") { }
}
