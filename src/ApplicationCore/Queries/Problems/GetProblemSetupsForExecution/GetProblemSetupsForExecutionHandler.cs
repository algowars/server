using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Problems.GetProblemSetupsForExecution;

public sealed class GetProblemSetupsForExecutionHandler(IProblemRepository problemRepository)
    : IQueryHandler<GetProblemSetupsForExecutionQuery, IEnumerable<ProblemSetupModel>>
{
    public async Task<Result<IEnumerable<ProblemSetupModel>>> Handle(
        GetProblemSetupsForExecutionQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Result.Success(
                await problemRepository.GetProblemSetupsAsync(request.SetupIds, cancellationToken)
            );
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}