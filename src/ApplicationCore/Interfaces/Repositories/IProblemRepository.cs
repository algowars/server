using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;

namespace ApplicationCore.Interfaces.Repositories;

public interface IProblemRepository
{
    Task<ProblemModel?> GetProblemBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<ProblemModel?> GetProblemByIdAsync(Guid problemId, CancellationToken cancellationToken);

    Task<IEnumerable<ProblemSetup>> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    );

    Task<ProblemSetup?> GetProblemSetupAsync(int setupId, CancellationToken cancellationToken);

    Task<PaginatedResult<ProblemModel>> GetProblemsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken
    );

    Task<PaginatedResult<ProblemModel>> GetAdminProblemsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken
    );

    Task<ICollection<ProgrammingLanguage>> GetAllLanguagesAsync(
        CancellationToken cancellationToken
    );

    Task<ProblemModel?> FindByTitleAsync(string title, CancellationToken cancellationToken);

    Task<Guid> CreateProblemAsync(ProblemModel problem, CancellationToken cancellationToken);
}
