using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

[DisallowConcurrentExecution]
internal sealed class PollSubmissionReportExecutionHandler(IServiceScopeFactory serviceScopeFactory)
    : JobBase
{
    public override JobType JobType => JobType.PollSubmissionReportExecution;

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

        var outboxes = outboxResults
            .Value.Where(outbox => outbox.Type == SubmissionOutboxType.PollEvaluation)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var submissionResults = await codeExecutionService.GetSubmissionReportResultsAsync(
            outboxes.Select(o => o.Submission),
            cancellationToken
        );

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;
        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        await submissionAppService.ProcessPollingSubmissionReportExecutionsAsync(
            submissionResults.Value,
            cancellationToken
        );
    }
}
