namespace ApplicationCore.Domain.Problems.Languages;

public class ProgrammingLanguage : BaseAuditableEntity<int>
{
    public required string Name { get; init; }
    
    public required bool IsArchived { get; init; }

    public ICollection<LanguageVersion> Versions { get; init; } = [];
}