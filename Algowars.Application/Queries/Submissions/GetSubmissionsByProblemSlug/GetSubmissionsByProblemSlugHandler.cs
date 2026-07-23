
using Algowars.Application.Pagination;
using Algowars.Application.Problems;
using Algowars.Application.Submissions;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;

namespace Algowars.Application.Queries.Submissions.GetSubmissionsByProblemSlug;

internal sealed class GetSubmissionsByProblemSlugHandler(IProblemReadRepository problemReadRepository, ISubmissionReadRepository submissionReadRepository) : IQueryHandler<GetSubmissionsByProblemSlugQuery, PageResult<ProblemSubmissionDto>>
{
    public async Task<Result<PageResult<ProblemSubmissionDto>>> Handle(
        GetSubmissionsByProblemSlugQuery request,
        CancellationToken cancellationToken)
    {
        var problemId = await problemReadRepository.GetIdBySlugAsync(request.ProblemSlug, cancellationToken);

        if (problemId is null)
        {
            return Result.NotFound();
        }

        var pageResult = await submissionReadRepository.GetProblemSubmissionsPagedAsync(
            problemId.Value,
            request.PaginationRequest,
            request.UserId,
            request.IncludeAllSubmissions,
            cancellationToken);

        return Result.Success(pageResult);
    }
}
