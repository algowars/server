namespace ApplicationCore.Domain.Submissions.Outboxes;

public sealed class SubmissionOutboxModel
{
    public Guid Id { get; init; }

    public required Guid SubmissionId { get; init; }

    public required SubmissionModel Submission { get; init; }

    public required SubmissionOutboxType Type { get; init; }

    public required SubmissionOutboxStatus Status { get; init; }
}