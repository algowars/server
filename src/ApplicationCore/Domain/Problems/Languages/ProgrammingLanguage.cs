namespace ApplicationCore.Domain.Problems.Languages;

public class ProgrammingLanguage : BaseAuditableModel<int>
{
    public required string Name { get; init; }

    public bool IsArchived { get; init; }

    public IEnumerable<LanguageVersion> Versions { get; init; } = [];
}