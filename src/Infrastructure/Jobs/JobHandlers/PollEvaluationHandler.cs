using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

/// <summary>
/// Recovery sweep for Stage 4: finalizes outboxes stuck in EvaluationPoll.
/// Mirrors <see cref="Infrastructure.Messaging.Consumers.SubmissionEvaluationPollConsumer"/>.
/// </summary>
[DisallowConcurrentExecution]
public sealed partial class PollEvaluationHandler(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PollEvaluationHandler> logger
) : JobBase
{
    public override JobType JobType => JobType.PollEvaluation;

    protected override ILogger Logger => logger;

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

        LogFinalizing(logger, outboxIds.Count);

        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);
        await submissionAppService.FinalizeEvaluationAsync(outboxIds, now, cancellationToken);
    }

    [LoggerMessage(
        EventId = LoggingEventIds.Jobs.PollEvaluationFinalizing,
        Level = LogLevel.Information,
        Message = "Finalizing {count} evaluation outboxes"
    )]
    private static partial void LogFinalizing(ILogger logger, int count);
}
