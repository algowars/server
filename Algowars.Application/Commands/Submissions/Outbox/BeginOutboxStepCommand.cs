using Algowars.Domain.Submissions.Outbox.Enums;

namespace Algowars.Application.Commands.Submissions.Outbox;

public sealed record BeginOutboxStepCommand(Guid SubmissionId, SubmissionOutboxStep Step) : ICommand<Guid>;
