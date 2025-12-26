namespace ApplicationCore.Dtos.Languages;

public sealed record ProgrammingLanguageDto
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public bool IsArchived { get; init; }

    public ICollection<LanguageVersionDto>? Versions { get; init; } = [];
}
