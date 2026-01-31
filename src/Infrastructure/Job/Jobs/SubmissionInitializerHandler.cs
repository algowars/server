namespace Infrastructure.Job.Jobs;

public sealed class SubmissionInitializerHandler : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionInitializer;

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
