namespace ApplicationCore.Domain.Submissions;

public sealed class SubmissionResult
{
    public required Guid Id { get; init; }

    public required SubmissionStatus Status { get; init; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? Stdout { get; set; }

    public int? RuntimeMs { get; set; }

    public int? MemoryKb { get; set; }
}
