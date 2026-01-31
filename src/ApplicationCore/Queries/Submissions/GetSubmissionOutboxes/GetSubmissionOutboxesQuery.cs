using ApplicationCore.Domain.Submissions.Outboxes;

namespace ApplicationCore.Queries.Submissions.GetSubmissionOutboxes;

public sealed record GetSubmissionOutboxesQuery() : IQuery<IEnumerable<SubmissionOutboxModel>>;
