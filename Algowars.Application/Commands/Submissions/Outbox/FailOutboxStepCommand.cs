namespace Algowars.Application.Commands.Submissions.Outbox;

public sealed record FailOutboxStepCommand(Guid OutboxId, string Error) : ICommand;
