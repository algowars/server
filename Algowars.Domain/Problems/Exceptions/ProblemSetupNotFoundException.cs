using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Exceptions;

public sealed class ProblemSetupNotFoundException(Guid setupId)
    : DomainException($"Problem setup '{setupId}' was not found.")
{
}
