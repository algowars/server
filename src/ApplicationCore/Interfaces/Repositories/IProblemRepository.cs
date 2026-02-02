using ApplicationCore.Commands.Problem.CreateProblem;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;

namespace ApplicationCore.Interfaces.Repositories;

public interface IProblemRepository
{
    Task<Guid> CreateProblemAsync(ProblemModel problem, CancellationToken cancellationToken);

    Task<IEnumerable<ProgrammingLanguage>> GetAvailableLanguagesAsync(
        CancellationToken cancellationToken
    );

    Task<ProblemModel?> GetProblemBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<ProblemSetupModel?> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    );

    Task<PaginatedResult<ProblemModel>> GetProblemsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken
    );

    Task<IEnumerable<ProblemSetupModel>> GetProblemSetupsAsync(
        IEnumerable<int> problemSetupIds,
        CancellationToken cancellationToken
    );
}