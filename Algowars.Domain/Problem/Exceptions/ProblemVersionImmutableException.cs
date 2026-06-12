using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class ProblemVersionImmutableException : DomainException
{
    public ProblemVersionImmutableException()
        : base("Cannot modify judging-critical fields on a published version.") { }
}
