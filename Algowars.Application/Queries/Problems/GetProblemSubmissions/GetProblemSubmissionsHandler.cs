using Algowars.Application.Submissions;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;

namespace Algowars.Application.Queries.Problems.GetProblemSubmissions;

internal sealed class GetProblemSubmissionsHandler(ISubmissionReadRepository submissionReadRepository)
    : IQueryHandler<GetProblemSubmissionsQuery, ProblemSubmissionsPageResult>
{
    public async Task<Result<ProblemSubmissionsPageResult>> Handle(
        GetProblemSubmissionsQuery request,
        CancellationToken cancellationToken)
    {
        var userIdFilter = request.IncludeAllSubmissions ? null : request.UserId;

        var result = await submissionReadRepository.GetProblemSubmissionsPagedAsync(
            request.ProblemId,
            userIdFilter,
            request.PaginationRequest,
            cancellationToken);

        return Result.Success(result);
    }
}
