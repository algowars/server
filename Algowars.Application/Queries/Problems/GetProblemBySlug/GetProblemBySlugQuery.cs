using Algowars.Application.Problems.Dtos;

namespace Algowars.Application.Queries.Problems.GetProblemBySlug;

public sealed record GetProblemBySlugQuery(string Slug) : IQuery<ProblemWithSetupsDto>;