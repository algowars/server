using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class InvalidMemoryLimitException(string reason) : DomainException($"Memory limit is invalid: {reason}")
{
}
