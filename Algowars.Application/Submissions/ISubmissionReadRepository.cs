using Algowars.Application.Pagination;
using Algowars.Application.Submissions.Dtos;

namespace Algowars.Application.Submissions;

public interface ISubmissionReadRepository
{
    Task<PageResult<ProblemSubmissionDto>> GetProblemSubmissionsPagedAsync(
        Guid problemId,
        PaginationRequest paginationRequest,
        Guid? userId,
        bool includeAllSubmissions = true,
        CancellationToken cancellationToken = default);
}
