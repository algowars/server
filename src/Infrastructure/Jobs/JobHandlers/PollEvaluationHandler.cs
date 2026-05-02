using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

/// <summary>
/// Recovery sweep for Stage 4: finalizes outboxes stuck in EvaluationPoll.
/// Mirrors <see cref="Infrastructure.Messaging.Consumers.SubmissionEvaluationPollConsumer"/>.
/// </summary>
[DisallowConcurrentExecution]
public sealed class PollEvaluationHandler(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.PollEvaluation;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxIds = outboxResults.Value
            .Where(o => o.Type == SubmissionOutboxType.EvaluationPoll)
            .Select(o => o.Id)
            .ToList();

        if (outboxIds.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);
        await submissionAppService.FinalizeEvaluationAsync(outboxIds, now, cancellationToken);
    }
}
