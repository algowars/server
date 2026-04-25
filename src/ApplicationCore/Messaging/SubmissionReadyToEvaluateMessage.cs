namespace ApplicationCore.Messaging;

public sealed record SubmissionReadyToEvaluateMessage
{
    public required Guid SubmissionId { get; init; }
    public required Guid OutboxId { get; init; }
}
