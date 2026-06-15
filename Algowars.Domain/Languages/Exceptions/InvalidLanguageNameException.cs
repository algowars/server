using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Exceptions;

public sealed class InvalidLanguageNameException(string reason)
    : DomainException($"Language name is invalid: {reason}")
{
}
