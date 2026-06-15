namespace Algowars.Application.Dtos.Submissions;

public sealed record SubmissionOutboxDto
{
    public required Guid Id { get; init; }
    public required Guid SubmissionId { get; init; }
    public required string Step { get; init; }
    public required string Status { get; init; }
    public required int AttemptCount { get; init; }
    public required int MaxAttempts { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastAttemptAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? LastError { get; init; }
}
