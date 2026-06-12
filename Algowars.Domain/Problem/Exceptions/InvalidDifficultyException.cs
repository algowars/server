using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class InvalidDifficultyException(string reason) : DomainException($"Difficulty is invalid: {reason}")
{
}
