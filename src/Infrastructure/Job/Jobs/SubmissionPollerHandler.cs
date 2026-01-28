using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionPollerHandler(
    ISubmissionAppService submissionAppService,
    ICodeExecutionService codeExecutionService,
    ISubmissionRepository submissionRepository
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionPoller;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var submissionOutboxResults = (
            await submissionRepository.GetSubmissionPollingOutboxesAsync(cancellationToken)
        ).ToList();

        if (submissionOutboxResults.Count == 0)
        {
            return;
        }

        var submissions = submissionOutboxResults.Select(outbox => outbox.Submission);

        var now = DateTime.UtcNow;
        await submissionRepository.IncrementOutboxesCount(
            submissionOutboxResults.Select(outbox => outbox.Id),
            now,
            cancellationToken
        );

        var polledResults = await codeExecutionService.GetSubmissionResultsAsync(
            submissions,
            cancellationToken
        );

        if (!polledResults.IsSuccess || !polledResults.Value.Any())
        {
            return;
        }

        await submissionRepository.ProcessSubmissionExecution(
            polledResults.Value,
            cancellationToken
        );
    }
}
