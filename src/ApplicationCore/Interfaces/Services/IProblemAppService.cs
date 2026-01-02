using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface IProblemAppService
{
    Task<Result<ProblemDto>> GetProblemBySlugAsync(
        string slug,
        CancellationToken cancellationToken
    );

    Task<Result<PaginatedResult<ProblemDto>>> GetProblemsPaginatedAsync(
        int pageNumber,
        int pageSize,
        DateTime timestamp,
        CancellationToken cancellationToken
    );

    Task<Result<PaginatedResult<ProblemDto>>> GetProblemsPaginatedAsync(
        int pageNumber,
        int pageSize,
        DateTime timestamp,
        string query,
        CancellationToken cancellationToken
    );

    Task<Result<ProblemSetupDto>> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    );
}
