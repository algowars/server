using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmissions;

public class GetSubmissionsHandler(ISubmissionRepository submissionRepository)
    : IQueryHandler<GetSubmissionsQuery, PaginatedResult<SubmissionModel>>
{
    public async Task<Result<PaginatedResult<SubmissionModel>>> Handle(
        GetSubmissionsQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var results = await submissionRepository.GetProblemSubmissions(
                request.ProblemId,
                request.PaginationRequest,
                cancellationToken
            );

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
