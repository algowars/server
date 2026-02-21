namespace ApplicationCore.Dtos.Languages;

public sealed record ProgrammingLanguageDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public bool IsArchived { get; init; }

    public IEnumerable<LanguageVersionDto> Versions { get; init; } = [];
}