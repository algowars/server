using Algowars.Application.Dtos.Problems;

namespace Algowars.Application.Queries.Problems.GetProblemBySlug;

internal sealed record GetProblemBySlugQuery(string Slug) : IQuery<ProblemDto>;
