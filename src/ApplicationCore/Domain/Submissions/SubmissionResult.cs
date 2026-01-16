namespace ApplicationCore.Domain.Submissions;

public sealed class SubmissionResult
{
    public required Guid Id { get; init; }

    public required SubmissionStatus Status { get; init; }

    public string? Stdout { get; set; }

    public string? Stderr { get; set; }

    public float? RuntimeMs { get; set; }

    public int? MemoryKb { get; set; }
}
