namespace Algowars.Application.Dtos.Languages;

public sealed record LanguageDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required bool IsActive { get; init; }
    public IEnumerable<LanguageVersionDto> Versions { get; init; } = [];
}
