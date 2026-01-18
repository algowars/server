using ApplicationCore.Domain.Submissions.Outbox;

namespace ApplicationCore.Queries.Submissions.GetSubmissionOutboxItems;

public sealed record GetSubmissionOutboxItemsQuery() : IQuery<IEnumerable<SubmissionOutboxModel>>;
