using Algowars.Application.Languages;
using Algowars.Application.Problems;
using Algowars.Application.Problems.Dtos;
using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algowars.Application.Queries.Problems.GetProblemBySlug;

internal sealed class GetProblemBySlugHandler(IProblemReadRepository problemReadRepository, ILanguageReadRepository languageReadRepository) : IQueryHandler<GetProblemBySlugQuery, ProblemWithSetupsDto>
{
    public async Task<Result<ProblemWithSetupsDto>> Handle(GetProblemBySlugQuery request, CancellationToken cancellationToken)
    {
        var problem = await problemReadRepository.FindBySlugAsync(request.Slug, cancellationToken);

        if (problem is null)
        {
            return Result.NotFound();
        }

        var languages = await languageReadRepository.FindLanguagesByVersionId(problem.AvailableLanguageVersionIds(), cancellationToken);

        return Result.Success(
            new ProblemWithSetupsDto(
                Id:  problem.Id,
                Slug: problem.Slug,
                Title: problem.Title,
                DifficultyTier: problem.Difficulty.Tier,
                Question: problem.Question,
                AvailableSetups: languages.Select(language => new ProblemSetupLanguageDto(
                    language.Id, language.Name, LanguageVersions: language.Versions.Select))))
    }
}
