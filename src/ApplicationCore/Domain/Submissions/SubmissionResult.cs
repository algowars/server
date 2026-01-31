namespace ApplicationCore.Domain.Submissions;

public sealed class SubmissionResult
{
    public Guid Id { get; set; }

    public required SubmissionStatus Status { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? Stdout { get; set; }

    public string? Stderr { get; set; }

    public int? RuntimeMs { get; set; }

    public int? MemoryKb { get; set; }
}
