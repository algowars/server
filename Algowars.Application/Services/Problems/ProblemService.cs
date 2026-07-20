using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;
using Algowars.Application.Queries.Problems.GetProblemBySlug;
using Algowars.Application.Queries.Problems.GetProblemSetup;
using Algowars.Application.Queries.Problems.GetProblemsPageable;
using Algowars.Application.Queries.Problems.GetProblemSubmissions;
using Algowars.Application.Submissions.Dtos;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Problems;

public interface IProblemService
{
    Task<Result<ProblemSetupDto>> GetProblemSetupAsync(string slug, Guid languageVersionId, CancellationToken cancellationToken);

    Task<Result<PageResult<ProblemDto>>> GetProblemsPageableAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);

    Task<Result<ProblemWithSetupsDto>> GetProblemWithSetupsBySlug(string slug, CancellationToken cancellationToken);

    Task<Result<ProblemSubmissionsPageResult>> GetProblemSubmissionsAsync(Guid problemId, PaginationRequest paginationRequest, Guid? userId, bool includeAllSubmissions, CancellationToken cancellationToken);
}

internal sealed class ProblemService(IMediator mediator) : IProblemService
{
    public async Task<Result<ProblemSetupDto>> GetProblemSetupAsync(string slug, Guid languageVersionId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProblemSetupQuery(slug, languageVersionId), cancellationToken);

        return result;
    }

    public async Task<Result<PageResult<ProblemDto>>> GetProblemsPageableAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProblemsPageableQuery(paginationRequest), cancellationToken);

        return result;
    }

    public async Task<Result<ProblemWithSetupsDto>> GetProblemWithSetupsBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProblemBySlugQuery(slug), cancellationToken);

        return result;
    }

    public async Task<Result<ProblemSubmissionsPageResult>> GetProblemSubmissionsAsync(Guid problemId, PaginationRequest paginationRequest, Guid? userId, bool includeAllSubmissions, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProblemSubmissionsQuery(problemId, paginationRequest, userId, includeAllSubmissions), cancellationToken);

        return result;
    }
}