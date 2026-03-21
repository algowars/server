using MediatR;

namespace ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;

public sealed record IncrementSubmissionOutboxesCommand(
    IEnumerable<Guid> OutboxIds,
    DateTime Timestamp
) : ICommand<Unit>;