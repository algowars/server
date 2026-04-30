namespace ApplicationCore.Messaging;

public sealed record SubmissionCreatedMessage
{
    public required Guid SubmissionId { get; init; }
    public required Guid OutboxId { get; init; }
}