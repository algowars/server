using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Dtos.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ISubmissionAppService
{
    Task<Result<Guid>> CreateAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    );

    Task<Result<PaginatedResult<SubmissionDto>>> GetSubmissionsAsync(
        Guid problemId,
        PaginationRequest paginationRequest,
        CancellationToken cancellationToken
    );
}
