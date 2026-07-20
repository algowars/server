using Algowars.Application.Pagination;
using Algowars.Application.Submissions.Dtos;

namespace Algowars.Application.Submissions;

public interface ISubmissionReadRepository
{
    Task<ProblemSubmissionsPageResult> GetProblemSubmissionsPagedAsync(
        Guid problemId,
        Guid? userId,
        PaginationRequest paginationRequest,
        CancellationToken cancellationToken = default);
}
