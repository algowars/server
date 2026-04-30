namespace ApplicationCore.Messaging;

public sealed record SubmissionExecutionPollMessage
{
    public required Guid SubmissionId { get; init; }
    public required Guid OutboxId { get; init; }
}