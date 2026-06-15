using Algowars.Application.Dtos.Problems;
using Algowars.Domain.Problems;
using Algowars.Domain.Problems.ValueObjects;
using Ardalis.Result;

namespace Algowars.Application.Queries.Problems.GetProblemBySlug;

internal sealed class GetProblemBySlugHandler(IProblemRepository problemRepository)
    : IQueryHandler<GetProblemBySlugQuery, ProblemDto>
{
    public async Task<Result<ProblemDto>> Handle(GetProblemBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = new Slug(request.Slug);
        var problem = await problemRepository.FindBySlugAsync(slug);
        if (problem is null)
            return Result.NotFound();

        var version = problem.CurrentVersion;
        return Result.Success(new ProblemDto
        {
            Id = problem.Id,
            Title = version?.Title.Value ?? string.Empty,
            Slug = problem.Slug.Value,
            Question = version?.Question.Value,
            Difficulty = version?.Difficulty.Value ?? 0,
            Version = version?.VersionNumber ?? 0,
            AvailableLanguages = [],
        });
    }
}
