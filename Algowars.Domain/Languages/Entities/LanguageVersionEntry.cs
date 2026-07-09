using Algowars.Domain.Languages.Enums;
using Algowars.Domain.Languages.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Languages.Entities;

public sealed class LanguageVersionEntry : Entity
{
    internal LanguageVersionEntry(LanguageVersion version, Judge0Id judge0Id)
    {
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Judge0Id = judge0Id ?? throw new ArgumentNullException(nameof(judge0Id));
        Status = LanguageVersionStatus.Active;
    }

    public void Deprecate()
    {
        Status = LanguageVersionStatus.Deprecated;
    }

    private LanguageVersionEntry() { }

    public bool IsActive => Status == LanguageVersionStatus.Active;
    public Judge0Id Judge0Id { get; private set; } = null!;
    public LanguageVersionStatus Status { get; private set; }
    public LanguageVersion Version { get; private set; } = null!;
}