using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class InvalidQuestionException(string reason) : DomainException($"Question is invalid: {reason}")
{
}
