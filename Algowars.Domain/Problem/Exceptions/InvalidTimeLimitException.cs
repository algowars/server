using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class InvalidTimeLimitException(string reason) : DomainException($"Time limit is invalid: {reason}")
{
}
