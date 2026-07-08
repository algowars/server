using Algowars.Application.Problems.Dtos;

namespace Algowars.Application.Queries.Problems.GetProblemSetup;

public sealed record GetProblemSetupQuery(string Slug, Guid LanguageVersionId) : IQuery<ProblemSetupDto>;