using Algowars.Domain.Languages.Enums;
using Algowars.Domain.Languages.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Entities;

public sealed class LanguageVersionEntry : Entity
{
    internal LanguageVersionEntry(LanguageVersion version)
    {
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Status = LanguageVersionStatus.Active;
    }

    public void Deprecate()
    {
        Status = LanguageVersionStatus.Deprecated;
    }

    private LanguageVersionEntry() { }

    public bool IsActive => Status == LanguageVersionStatus.Active;
    public LanguageVersionStatus Status { get; private set; }
    public LanguageVersion Version { get; private set; } = null!;
}
