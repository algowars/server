using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionExecutorJob(ISubmissionAppService submissionAppService)
    : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionExecutor;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var outboxSubmission = await submissionAppService.GetExecutionOutboxesAsync(
            cancellationToken
        );
    }
}
