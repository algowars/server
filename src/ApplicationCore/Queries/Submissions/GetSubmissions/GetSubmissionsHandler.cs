using ApplicationCore.Domain.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmissions;

public class GetSubmissionsHandler
    : IQueryHandler<GetSubmissionsQuery, IEnumerable<SubmissionModel>>
{
    public Task<Result<IEnumerable<SubmissionModel>>> Handle(
        GetSubmissionsQuery request,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
