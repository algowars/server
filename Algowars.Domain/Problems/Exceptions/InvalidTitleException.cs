using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Exceptions;

public sealed class InvalidTitleException(string reason) : DomainException($"Title is invalid: {reason}")
{
}
