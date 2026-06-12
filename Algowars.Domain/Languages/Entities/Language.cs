using Algowars.Domain.Languages.Enums;
using Algowars.Domain.Languages.Exceptions;
using Algowars.Domain.Languages.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Entities;

public sealed class Language : AggregateRoot
{
    public Language(LanguageName name, LanguageSlug slug)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Status = LanguageStatus.Active;
    }

    public void Activate()
    {
        Status = LanguageStatus.Active;
    }

    public LanguageVersionEntry AddVersion(LanguageVersion version)
    {
        var entry = new LanguageVersionEntry(version);
        _versions.Add(entry);
        return entry;
    }

    public void Deactivate()
    {
        Status = LanguageStatus.Inactive;
    }

    public void DeprecateVersion(Guid versionId)
    {
        var version = _versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw new LanguageVersionNotFoundException(versionId);

        version.Deprecate();
    }

    private Language() { }

    public bool IsActive => Status == LanguageStatus.Active;
    public LanguageName Name { get; private set; } = null!;
    public LanguageSlug Slug { get; private set; } = null!;
    public LanguageStatus Status { get; private set; }
    public IReadOnlyCollection<LanguageVersionEntry> Versions => _versions.AsReadOnly();

    private readonly List<LanguageVersionEntry> _versions = [];
}
