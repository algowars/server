using Algowars.Application.Common.Pagination;
using Algowars.Application.Dtos.Problems;
using Algowars.Domain.Problems;
using Ardalis.Result;

namespace Algowars.Application.Queries.Problems.GetProblemsPageable;

internal sealed class GetProblemsPageableHandler(IProblemRepository problemRepository)
    : IQueryHandler<GetProblemsPageableQuery, PaginatedResult<ProblemDto>>
{
    public async Task<Result<PaginatedResult<ProblemDto>>> Handle(GetProblemsPageableQuery request, CancellationToken cancellationToken)
    {
        var page = await problemRepository.GetPageAsync(request.Pagination.Page, request.Pagination.Size, cancellationToken);

        var dtos = page.Items.Select(p => new ProblemDto
        {
            Id = p.Id,
            Title = p.CurrentVersion?.Title.Value ?? string.Empty,
            Slug = p.Slug.Value,
            Question = p.CurrentVersion?.Question.Value,
            Difficulty = p.CurrentVersion?.Difficulty.Value ?? 0,
            Version = p.CurrentVersion?.VersionNumber ?? 0,
            AvailableLanguages = [],
        }).ToList();

        return Result.Success(new PaginatedResult<ProblemDto>
        {
            Results = dtos,
            Total = page.Total,
            Page = request.Pagination.Page,
            Size = request.Pagination.Size,
        });
    }
}
