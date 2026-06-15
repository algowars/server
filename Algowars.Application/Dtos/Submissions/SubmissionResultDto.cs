namespace Algowars.Application.Dtos.Submissions;

public sealed record SubmissionResultDto
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public int? RuntimeMs { get; init; }
    public int? MemoryKb { get; init; }
    public string? ActualOutput { get; init; }
    public string? StandardError { get; init; }
    public string? CompileOutput { get; init; }
}
