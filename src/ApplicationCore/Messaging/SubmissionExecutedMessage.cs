namespace ApplicationCore.Messaging;

public sealed record SubmissionExecutedMessage
{
    public required Guid SubmissionId { get; init; }
    public required Guid OutboxId { get; init; }
}