using ApplicationCore.Domain.Problems.ProblemSetups;

namespace ApplicationCore.Queries.Problems.GetProblemSetupsForExecution;

public sealed record GetProblemSetupsForExecutionQuery(IEnumerable<int> SetupIds)
    : IQuery<IEnumerable<ProblemSetupModel>>;
