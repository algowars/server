using ApplicationCore.Dtos.Problems;

namespace ApplicationCore.Queries.Problems.GetProblemBySlug;

public sealed record GetProblemBySlugQuery(string Slug) : IQuery<ProblemDto>;
