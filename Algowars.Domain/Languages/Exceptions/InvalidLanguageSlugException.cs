using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Exceptions;

public sealed class InvalidLanguageSlugException(string reason) : DomainException($"Language slug is invalid: {reason}")
{
}