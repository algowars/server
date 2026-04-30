using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Stage 2: Poll Judge0 for results using the stored ExecutionId tokens.
/// Increments the attempt count, persists stdout/status, transitions
/// outbox PollExecution → Evaluate once all results are finished, then
/// publishes <see cref="SubmissionReadyToEvaluateMessage"/>.
/// If results are still processing, re-publishes <see cref="SubmissionExecutionPollMessage"/>
/// to poll again on the next delivery.
/// </summary>
public sealed class SubmissionExecutedConsumer(IServiceScopeFactory serviceScopeFactory)
    : IConsumer<SubmissionExecutionPollMessage>
{
    public async Task Consume(ConsumeContext<SubmissionExecutionPollMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var codeExecutionService = scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outbox = outboxResults.Value.FirstOrDefault(o =>
            o.SubmissionId == context.Message.SubmissionId
            && o.Type == SubmissionOutboxType.PollExecution);

        if (outbox is null)
        {
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
            return;
        }

        var submission = pollResult.Value.FirstOrDefault();

        if (submission is null)
        {
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
    }
}