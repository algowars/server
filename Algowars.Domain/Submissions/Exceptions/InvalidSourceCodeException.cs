using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Submissions.Exceptions;

public sealed class InvalidSourceCodeException(string reason)
    : DomainException($"Source code is invalid: {reason}")
{
}
