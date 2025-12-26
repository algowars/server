using ApplicationCore.Dtos.Languages;

namespace ApplicationCore.Dtos.Problems;

public record ProblemDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public string? Question { get; init; }

    public required IEnumerable<string> Tags { get; init; }

    public required int Difficulty { get; init; }

    public required int Version { get; init; }

    public required IEnumerable<ProgrammingLanguageDto> AvailableLanguages { get; init; }
}
