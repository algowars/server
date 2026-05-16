using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Logging;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Stage 4: Final stage — increments attempt count and sets FinalizedOn
/// on the outbox, completing the submission pipeline.
/// </summary>
public sealed partial class SubmissionEvaluationPollConsumer(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SubmissionEvaluationPollConsumer> logger
) : IConsumer<SubmissionEvaluationPollMessage>
{
    private readonly ILogger<SubmissionEvaluationPollConsumer> _logger = logger;
    public async Task Consume(ConsumeContext<SubmissionEvaluationPollMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        LogStage4Started(context.Message.SubmissionId, context.Message.OutboxId);

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            LogStage4OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        var outbox = outboxResults.Value.FirstOrDefault(o =>
            o.SubmissionId == context.Message.SubmissionId
            && o.Type == SubmissionOutboxType.EvaluationPoll);

        if (outbox is null)
        {
            LogStage4OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync([outbox.Id], now, cancellationToken);
        await submissionAppService.FinalizeEvaluationAsync([outbox.Id], now, cancellationToken);

        LogStage4Completed(context.Message.SubmissionId, context.Message.OutboxId);
    }

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage4Started, Level = LogLevel.Information,
        Message = "Stage4: Finalizing evaluation for submission {submissionId} (outbox {outboxId})")]
    private partial void LogStage4Started(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage4OutboxNotFound, Level = LogLevel.Warning,
        Message = "Stage4: Outbox not found or not in EvaluationPoll state for submission {submissionId} (outbox {outboxId}) — may have already been processed")]
    private partial void LogStage4OutboxNotFound(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage4Completed, Level = LogLevel.Information,
        Message = "Stage4: Submission {submissionId} finalized (outbox {outboxId})")]
    private partial void LogStage4Completed(Guid submissionId, Guid outboxId);
}