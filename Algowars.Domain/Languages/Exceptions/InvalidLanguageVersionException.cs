using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Exceptions;

public sealed class InvalidLanguageVersionException(string reason) : DomainException($"Language version is invalid: {reason}")
{
}
