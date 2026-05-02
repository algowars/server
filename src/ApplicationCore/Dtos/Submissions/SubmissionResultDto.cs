using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Dtos.Submissions;

public sealed class SubmissionResultDto
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public int? RuntimeMs { get; init; }
    public int? MemoryKb { get; init; }
    public DateTime? FinishedAt { get; init; }
}
