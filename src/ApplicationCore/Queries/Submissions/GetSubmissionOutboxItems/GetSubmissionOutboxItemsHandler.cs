using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmissionOutboxItems;

public sealed class GetSubmissionOutboxItemsHandler(ISubmissionRepository submissionRepository)
    : IQueryHandler<GetSubmissionOutboxItemsQuery, IEnumerable<SubmissionOutboxModel>>
{
    public Task<Result<IEnumerable<SubmissionOutboxModel>>> Handle(
        GetSubmissionOutboxItemsQuery request,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
