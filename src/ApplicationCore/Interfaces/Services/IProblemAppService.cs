using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Dtos.Problems.Admin;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface IProblemAppService
{
    Task<Result<Guid>> CreateProblemAsync(
        CreateProblemDto createProblemDto,
        Guid createdById,
        CancellationToken cancellationToken
    );

    Task<PaginatedResult<AdminProblemDto>> GetAdminProblemsPageableAsync(
        int pageNumber,
        int pageSize,
        DateTime timestamp,
        CancellationToken cancellationToken
    );

    Task<Result<IEnumerable<ProgrammingLanguageDto>>> GetAvailableLanguagesAsync(
        CancellationToken cancellationToken
    );

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

    Task<Result<IEnumerable<ProblemSetupModel>>> GetProblemSetupsForExecutionAsync(
        IEnumerable<int> setupIds,
        CancellationToken cancellationToken
    );

    Task<Result<ProblemSetupDto>> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    );
}