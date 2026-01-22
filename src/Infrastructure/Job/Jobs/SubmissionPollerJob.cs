using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionPollerJob(
    ISubmissionAppService submissionAppService,
    ICodeExecutionService codeExecutionService,
    ISubmissionRepository submissionRepository
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionPoller;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var submissionOutboxResults = await submissionAppService.GetOutboxesAsync(
            cancellationToken
        );

        if (!submissionOutboxResults.IsSuccess)
        {
            return;
        }

        var submissionThatNeedPolling = submissionOutboxResults.Value.Where(outbox =>
            outbox.Type == SubmissionOutboxType.PollJudge0Result
        );

        var submissions = submissionThatNeedPolling.Select(outbox => outbox.Submission);

        var polledResults = await codeExecutionService.GetSubmissionResultsAsync(
            submissions,
            cancellationToken
        );

        if (!polledResults.IsSuccess || !polledResults.Value.Any())
        {
            return;
        }

        await submissionRepository.BulkUpsertResultsAsync(polledResults.Value, cancellationToken);
    }
}
