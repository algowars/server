using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using Mapster;

namespace ApplicationCore.Queries.Problems.GetProblemBySlug;

public sealed class GetProblemBySlugHandler(IProblemRepository problemRepository)
    : IQueryHandler<GetProblemBySlugQuery, ProblemDto>
{
    public async Task<Result<ProblemDto>> Handle(
        GetProblemBySlugQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var problem = await problemRepository.GetProblemBySlugAsync(
                request.Slug,
                cancellationToken
            );

            return problem is null
                ? Result.NotFound()
                : Result.Success(problem.Adapt<ProblemDto>());
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}