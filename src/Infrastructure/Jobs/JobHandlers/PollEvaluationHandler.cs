using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

[DisallowConcurrentExecution]
public sealed class PollEvaluationHandler(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.PollEvaluation;

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
            .Where(outbox => outbox.Type == SubmissionOutboxType.PollEvaluation)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        var evaluationResults = await codeExecutionService.GetEvaluationResultsAsync(
            outboxes.Select(outbox => outbox.Submission),
            cancellationToken
        );

        if (!evaluationResults.IsSuccess)
        {
            return;
        }

        await submissionAppService.ProcessPollExecutionAsync(
            evaluationResults.Value,
            cancellationToken
        );
    }
}
