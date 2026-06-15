using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Exceptions;

public sealed class InvalidSlugException(string reason) : DomainException($"Slug is invalid: {reason}")
{
}
