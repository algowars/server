using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class InvalidTitleException(string reason) : DomainException($"Title is invalid: {reason}")
{
}
