namespace ApplicationCore.Dtos.Languages;

public sealed record LanguageVersionDto
{
    public required int Id { get; init; }

    public required string Version { get; init; }

    public string? InitialCode { get; init; }

    public ProgrammingLanguageDto? ProgrammingLanguage { get; init; }
}
