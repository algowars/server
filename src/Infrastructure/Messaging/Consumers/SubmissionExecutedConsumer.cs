using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Logging;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Stage 2: Poll Judge0 for results using the stored ExecutionId tokens.
/// Increments the attempt count, persists stdout/status, transitions
/// outbox PollExecution → Evaluate once all results are finished, then
/// publishes <see cref="SubmissionReadyToEvaluateMessage"/>.
/// If results are still processing, re-publishes <see cref="SubmissionExecutionPollMessage"/>
/// to poll again on the next delivery.
/// </summary>
public sealed partial class SubmissionExecutedConsumer(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SubmissionExecutedConsumer> logger
) : IConsumer<SubmissionExecutionPollMessage>
{
    private readonly ILogger<SubmissionExecutedConsumer> _logger = logger;
    public async Task Consume(ConsumeContext<SubmissionExecutionPollMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        LogStage2Started(context.Message.SubmissionId, context.Message.OutboxId);

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var codeExecutionService = scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            LogStage2OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        var outbox = outboxResults.Value.FirstOrDefault(o =>
            o.SubmissionId == context.Message.SubmissionId
            && o.Type == SubmissionOutboxType.PollExecution);

        if (outbox is null)
        {
            LogStage2OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        var now = DateTime.UtcNow;
        await submissionAppService.IncrementOutboxesCountAsync([outbox.Id], now, cancellationToken);

        var pollResult = await codeExecutionService.GetSubmissionResultsAsync(
            [outbox.Submission],
            cancellationToken
        );

        if (!pollResult.IsSuccess)
        {
            LogStage2PollFailed(context.Message.SubmissionId, string.Join(", ", pollResult.Errors));
            return;
        }

        var submission = pollResult.Value.FirstOrDefault();

        if (submission is null)
        {
            LogStage2PollFailed(context.Message.SubmissionId, "No submission returned from poll");
            return;
        }

        await submissionAppService.ProcessPollingSubmissionExecutionsAsync(
            pollResult.Value,
            cancellationToken
        );

        bool allFinished = submission.Results.All(r =>
            r.Status is not SubmissionStatus.InQueue
            and not SubmissionStatus.Processing);

        if (!allFinished)
        {
            LogStage2StillProcessing(context.Message.SubmissionId);

            await context.Publish(new SubmissionExecutionPollMessage
            {
                SubmissionId = context.Message.SubmissionId,
                OutboxId = context.Message.OutboxId,
            }, cancellationToken);

            return;
        }

        await context.Publish(new SubmissionReadyToEvaluateMessage
        {
            SubmissionId = context.Message.SubmissionId,
            OutboxId = context.Message.OutboxId,
        }, cancellationToken);

        LogStage2Completed(context.Message.SubmissionId, context.Message.OutboxId);
    }

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage2Started, Level = LogLevel.Information,
        Message = "Stage2: Polling execution results for submission {submissionId} (outbox {outboxId})")]
    private partial void LogStage2Started(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage2OutboxNotFound, Level = LogLevel.Warning,
        Message = "Stage2: Outbox not found or not in PollExecution state for submission {submissionId} (outbox {outboxId}) — may have already been processed")]
    private partial void LogStage2OutboxNotFound(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage2PollFailed, Level = LogLevel.Error,
        Message = "Stage2: Poll failed for submission {submissionId}: {errors}")]
    private partial void LogStage2PollFailed(Guid submissionId, string errors);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage2StillProcessing, Level = LogLevel.Information,
        Message = "Stage2: Submission {submissionId} still processing, re-queuing poll")]
    private partial void LogStage2StillProcessing(Guid submissionId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage2Completed, Level = LogLevel.Information,
        Message = "Stage2: All results received for submission {submissionId} (outbox {outboxId}), advancing to evaluation")]
    private partial void LogStage2Completed(Guid submissionId, Guid outboxId);
}