namespace Algowars.Application.Commands.Submissions.Outbox;

public sealed record RecordOutboxAttemptCommand(Guid OutboxId) : ICommand;
