using Algowars.Application.Dtos.Problems;

namespace Algowars.Application.Queries.Problems.GetProblemBySlug;

public sealed record GetProblemBySlugQuery(string Slug) : IQuery<ProblemDto>;
