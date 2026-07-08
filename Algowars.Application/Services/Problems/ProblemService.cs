using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;
using Algowars.Application.Queries.Problems.GetProblemBySlug;
using Algowars.Application.Queries.Problems.GetProblemSetup;
using Algowars.Application.Queries.Problems.GetProblemsPageable;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Problems;

public interface IProblemService
{
    Task<Result<ProblemSetupDto>> GetProblemSetupAsync(string slug, Guid languageVersionId, CancellationToken cancellationToken);

    Task<Result<PageResult<ProblemDto>>> GetProblemsPageableAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);

    Task<Result<ProblemWithSetupsDto>> GetProblemWithSetupsBySlug(string slug, CancellationToken cancellationToken);
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
}
