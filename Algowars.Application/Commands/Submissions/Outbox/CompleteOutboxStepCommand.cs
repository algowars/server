namespace Algowars.Application.Commands.Submissions.Outbox;

public sealed record CompleteOutboxStepCommand(Guid OutboxId) : ICommand;
