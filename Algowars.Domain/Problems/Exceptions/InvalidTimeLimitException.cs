using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Exceptions;

public sealed class InvalidTimeLimitException(string reason) : DomainException($"Time limit is invalid: {reason}")
{
}
