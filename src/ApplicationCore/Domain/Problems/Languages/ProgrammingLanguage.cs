namespace ApplicationCore.Domain.Problems.Languages;

public class ProgrammingLanguage : BaseAuditableModel<int>
{
    public required string Name { get; init; }

    public required bool IsArchived { get; init; }

    public ICollection<LanguageVersion> Versions { get; init; } = [];
}
