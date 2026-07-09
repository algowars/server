using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Exceptions;

public sealed class InvalidQuestionException(string reason) : DomainException($"Question is invalid: {reason}")
{
}