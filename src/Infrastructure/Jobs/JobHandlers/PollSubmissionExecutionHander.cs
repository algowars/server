using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

[DisallowConcurrentExecution]
internal sealed partial class PollSubmissionExecutionHander(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PollSubmissionExecutionHander> logger
) : JobBase
{
    public override JobType JobType => JobType.PollSubmissionExecution;

    protected override ILogger Logger => logger;

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
            .Value.Where(outbox => outbox.Type == SubmissionOutboxType.PollExecution)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        LogPolling(logger, outboxes.Count);

        var submissionResults = await codeExecutionService.GetSubmissionResultsAsync(
            outboxes.Select(o => o.Submission),
            cancellationToken
        );

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;
        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        await submissionAppService.ProcessPollingSubmissionExecutionsAsync(
            submissionResults.Value,
            cancellationToken
        );
    }

    [LoggerMessage(
        EventId = LoggingEventIds.Jobs.PollSubmissionExecutionPolling,
        Level = LogLevel.Information,
        Message = "Polling {count} submission execution outboxes"
    )]
    private static partial void LogPolling(ILogger logger, int count);
}