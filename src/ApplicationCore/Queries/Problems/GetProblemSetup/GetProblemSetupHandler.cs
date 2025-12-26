using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Problems.GetProblemBySlug;
using Ardalis.Result;

namespace ApplicationCore.Queries.Problems.GetProblemSetup;

public sealed class GetProblemBySlugHandler(IProblemRepository problemRepository)
    : IQueryHandler<GetProblemBySlugQuery, ProblemDto>
{
    private readonly IProblemRepository _problemRepository = problemRepository;

    public async Task<Result<ProblemDto>> Handle(
        GetProblemBySlugQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var problem = await _problemRepository.GetProblemBySlugAsync(
                request.Slug,
                cancellationToken
            );

            if (problem is null)
            {
                return Result.NotFound();
            }

            var dto = new ProblemDto
            {
                Id = problem.Id,
                Title = problem.Title,
                Slug = problem.Slug,
                Tags = problem.Tags.Select(t => t.Value),
                Question = problem.Question,
                Difficulty = problem.Difficulty,
                Version = problem.Version,
                AvailableLanguages = problem
                    .GetAvailableLanguages()
                    .Select(language => new ProgrammingLanguageDto()
                    {
                        Id = language.Id,
                        Name = language.Name,
                        Versions = language
                            .Versions.Select(version => new LanguageVersionDto()
                            {
                                Id = version.Id,
                                Version = version.Version,
                            })
                            .ToList(),
                    }),
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
