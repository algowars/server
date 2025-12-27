namespace ApplicationCore.Domain.Problems.Languages;

public sealed class LanguageVersion : BaseModel<int>
{
    public required string Version { get; init; }

    public string? InitialCode { get; init; }

    public int ProgrammingLanguageId { get; init; }
    public ProgrammingLanguage? ProgrammingLanguage { get; init; }
}
