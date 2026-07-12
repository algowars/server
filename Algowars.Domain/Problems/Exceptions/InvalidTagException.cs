using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Exceptions;

public sealed class InvalidTagException(string reason) : DomainException($"Tag is invalid: {reason}")
{
}
