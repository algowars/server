using ApplicationCore.Dtos.Problems;

namespace ApplicationCore.Queries.Problems.GetProblemSetup;

public sealed record GetProblemSetupQuery(Guid ProblemId, int LanguageVersionId)
    : IQuery<ProblemSetupDto>;
