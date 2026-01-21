using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionPollerJob(
    ISubmissionAppService submissionAppService,
    IProblemAppService problemAppService,
    ICodeBuilderService codeBuilderService,
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

        var tokens = submissionThatNeedPolling.Select(outbox => outbox.Submission);

        var polledResults = await codeExecutionService.GetSubmissionResultsAsync(
            tokens,
            cancellationToken
        );

        if (!polledResults.IsSuccess || !polledResults.Value.Any())
        {
            return;
        }

        await submissionRepository.BulkUpsertResultsAsync(
            polledResults.Value.SelectMany(s => s.Results),
            cancellationToken
        );
    }
}
