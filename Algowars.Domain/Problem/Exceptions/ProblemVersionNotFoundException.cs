using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Exceptions;

public sealed class ProblemVersionNotFoundException(Guid versionId) : DomainException($"Problem version '{versionId}' was not found.")
{
}
