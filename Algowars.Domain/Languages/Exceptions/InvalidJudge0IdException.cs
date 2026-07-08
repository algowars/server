using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Exceptions;

public sealed class InvalidJudge0IdException(string message) : DomainException(message)
{
}
