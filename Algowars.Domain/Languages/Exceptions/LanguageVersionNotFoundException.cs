using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Exceptions;

public sealed class LanguageVersionNotFoundException(Guid versionId) : DomainException($"Language version with ID '{versionId}' was not found.")
{
}
