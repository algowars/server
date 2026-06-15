namespace Algowars.Application.Dtos.Languages;

public sealed record LanguageVersionDto
{
    public required Guid Id { get; init; }
    public required string Version { get; init; }
    public required bool IsActive { get; init; }
}
