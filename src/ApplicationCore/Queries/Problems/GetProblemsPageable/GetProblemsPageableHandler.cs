using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Problems.GetProblemsPageable;

public sealed class GetProblemsPageableHandler(IProblemRepository repository)
    : IQueryHandler<GetProblemsPageableQuery, PaginatedResult<ProblemDto>>
{
    private readonly IProblemRepository _repository = repository;

    public async Task<Result<PaginatedResult<ProblemDto>>> Handle(
        GetProblemsPageableQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var problemPage = await _repository.GetProblemsAsync(
                request.Pagination,
                cancellationToken
            );

            var dtoItems = (problemPage.Results ?? [])
                .Select(p => new ProblemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    Tags = (p.Tags ?? []).Select(t => t.Value).ToList(),
                    Difficulty = p.Difficulty,
                    Version = p.Version,
                    AvailableLanguages = [],
                })
                .ToList();

            var dtoPage = new PaginatedResult<ProblemDto>
            {
                Results = dtoItems,
                Total = problemPage.Total,
                Page = problemPage.Page,
                Size = problemPage.Size,
            };

            return Result.Success(dtoPage);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
