using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;
using Algowars.Application.Queries.Problems.GetProblemsPageable;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Services.Problems;

public interface IProblemService
{
    Task<Result<PageResult<ProblemDto>>> GetProblemsPageableAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
}

internal sealed class ProblemService(IMediator mediator) : IProblemService
{
    public async Task<Result<PageResult<ProblemDto>>> GetProblemsPageableAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProblemsPageableQuery(paginationRequest), cancellationToken);

        return result;
    }
}
