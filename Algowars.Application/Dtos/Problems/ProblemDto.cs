using Algowars.Application.Dtos.Languages;

namespace Algowars.Application.Dtos.Problems;

public sealed record ProblemDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? Question { get; init; }
    public required int Difficulty { get; init; }
    public required int Version { get; init; }
    public required IEnumerable<LanguageDto> AvailableLanguages { get; init; }
}
