using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

[DisallowConcurrentExecution]
internal sealed class SubmissionPollerHandler(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.SubmissionPoller;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService =
            scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var codeExecutionService =
            scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(
            cancellationToken
        );

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults.Value
            .Where(outbox => outbox.Type == SubmissionOutboxType.PollExecution)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        var submissionResults = await codeExecutionService.GetSubmissionResultsAsync(
            outboxes.Select(outbox => outbox.Submission),
            cancellationToken
        );

        if (!submissionResults.IsSuccess)
        {
            return;
        }

        await submissionAppService.ProcessSubmissionPollingAsync(
            submissionResults.Value,
            cancellationToken
        );
    }
}

