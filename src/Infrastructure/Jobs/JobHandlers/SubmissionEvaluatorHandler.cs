using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

[DisallowConcurrentExecution]
public sealed class SubmissionEvaluatorHandler(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.SubmissionEvaluator;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService =
            scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var comparisonBuildingService =
            scope.ServiceProvider.GetRequiredService<IComparisonBuildingService>();
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
            .Where(outbox => outbox.Type == SubmissionOutboxType.EvaluateSubmission)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var buildResult = comparisonBuildingService.BuildComparisonContexts(outboxes);

        if (!buildResult.IsSuccess)
        {
            return;
        }

        var comparisonContexts = buildResult.Value.ToList();

        if (comparisonContexts.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        var evaluationResult = await codeExecutionService.EvaluateAsync(
            comparisonContexts,
            cancellationToken
        );

        if (!evaluationResult.IsSuccess)
        {
            return;
        }

        await submissionAppService.ProcessEvaluationAsync(
            evaluationResult.Value,
            cancellationToken
        );
    }
}
