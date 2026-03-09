using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs.JobHandlers;

internal sealed class SubmissionEvaluatorHandler() : JobBase
{
    public override JobType JobType => JobType.SubmissionEvaluator;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
