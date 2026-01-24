using ApplicationCore.Domain.Submissions.Outbox;

namespace ApplicationCore.Queries.Submissions.GetSubmissionExecutionOutboxes;

public sealed record GetSubmissionExecutionOutboxesCommand()
    : IQuery<IEnumerable<SubmissionOutboxModel>>;
