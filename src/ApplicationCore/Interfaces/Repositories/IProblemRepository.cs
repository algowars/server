using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;

namespace ApplicationCore.Interfaces.Repositories;

public interface IProblemRepository
{
    Task<ProblemModel?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken);

    Task<ProblemModel?> GetProblemBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<ProblemSetupModel> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    );

    Task<ProblemSetupModel?> GetProblemSetupAsync(int setupId, CancellationToken cancellationToken);

    Task<PaginatedResult<ProblemModel>> GetProblemsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken
    );
}
