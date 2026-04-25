namespace ApplicationCore.Messaging;

public sealed record SubmissionEvaluationPollMessage
{
    public required Guid SubmissionId { get; init; }
    public required Guid OutboxId { get; init; }
}
