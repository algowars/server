namespace Infrastructure.Jobs.JobHandlers;

internal class SubmissionPollerHandler : JobBase
{
    public override JobType JobType => JobType.SubmissionPoller;

    protected override Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
