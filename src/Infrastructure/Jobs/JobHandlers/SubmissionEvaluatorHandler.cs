using ApplicationCore.Jobs;

namespace Infrastructure.Jobs.JobHandlers;

internal sealed class SubmissionEvaluatorHandler : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionEvaluator;

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
