using Algowars.Application.Pagination;
using Algowars.Application.Problems;
using Algowars.Application.Problems.Dtos;
using Ardalis.Result;

namespace Algowars.Application.Queries.Problems.GetProblemsPageable;

internal sealed class GetProblemsPageableHandler(IProblemReadRepository problemReadRepository) : IQueryHandler<GetProblemsPageableQuery, PageResult<ProblemDto>>
{
    public async Task<Result<PageResult<ProblemDto>>> Handle(GetProblemsPageableQuery request, CancellationToken cancellationToken)
    {
        var result = await problemReadRepository.GetPagedAsync(request.PaginationRequest, cancellationToken);

        return Result.Success(result);
    }
}