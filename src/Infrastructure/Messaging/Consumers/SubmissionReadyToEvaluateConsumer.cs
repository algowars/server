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
/// Stage 3: Compare each result's stdout against its expected output using
/// <see cref="IExecutionComparisonService"/>. Persists Accepted/WrongAnswer
/// statuses, transitions outbox Evaluate → EvaluationPoll, then publishes
/// <see cref="SubmissionEvaluationPollMessage"/>.
/// </summary>
public sealed partial class SubmissionReadyToEvaluateConsumer(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SubmissionReadyToEvaluateConsumer> logger
) : IConsumer<SubmissionReadyToEvaluateMessage>
{
    private readonly ILogger<SubmissionReadyToEvaluateConsumer> _logger = logger;
    public async Task Consume(ConsumeContext<SubmissionReadyToEvaluateMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        LogStage3Started(context.Message.SubmissionId, context.Message.OutboxId);

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var problemAppService = scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        var comparisonService = scope.ServiceProvider.GetRequiredService<IExecutionComparisonService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            LogStage3OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        var outbox = outboxResults.Value.FirstOrDefault(o =>
            o.SubmissionId == context.Message.SubmissionId
            && o.Type == SubmissionOutboxType.Evaluate);

        if (outbox is null)
        {
            LogStage3OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        // Reload the problem setup to get expected outputs per test case
        var setupResult = await problemAppService.GetProblemSetupsForExecutionAsync(
            [outbox.Submission.ProblemSetupId],
            cancellationToken
        );

        if (!setupResult.IsSuccess)
        {
            LogStage3SetupNotFound(context.Message.SubmissionId, outbox.Submission.ProblemSetupId);
            return;
        }

        var setup = setupResult.Value.FirstOrDefault(s => s.Id == outbox.Submission.ProblemSetupId);

        if (setup is null)
        {
            LogStage3SetupNotFound(context.Message.SubmissionId, outbox.Submission.ProblemSetupId);
            return;
        }

        var expectedOutputs = setup.TestSuites
            .SelectMany(ts => ts.TestCases)
            .Select(tc => tc.ExpectedOutput.Value)
            .ToList();

        var results = outbox.Submission.Results.ToList();

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].Status != SubmissionStatus.Processing)
            {
                continue;
            }

            string expected = i < expectedOutputs.Count ? expectedOutputs[i] : string.Empty;
            results[i].Status = comparisonService.Compare(results[i].ProgramOutput, expected);
        }

        var evaluated = new SubmissionModel
        {
            Id = outbox.SubmissionId,
            CreatedById = outbox.Submission.CreatedById,
            Results = results,
        };

        var now = DateTime.UtcNow;
        await submissionAppService.IncrementOutboxesCountAsync([outbox.Id], now, cancellationToken);

        // Persist comparison results and transition Evaluate → EvaluationPoll
        await submissionAppService.ProcessEvaluationAsync([evaluated], cancellationToken);

        await context.Publish(new SubmissionEvaluationPollMessage
        {
            SubmissionId = context.Message.SubmissionId,
            OutboxId = context.Message.OutboxId,
        }, cancellationToken);

        LogStage3Completed(context.Message.SubmissionId, context.Message.OutboxId);
    }

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage3Started, Level = LogLevel.Information,
        Message = "Stage3: Starting evaluation for submission {submissionId} (outbox {outboxId})")]
    private partial void LogStage3Started(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage3OutboxNotFound, Level = LogLevel.Warning,
        Message = "Stage3: Outbox not found or not in Evaluate state for submission {submissionId} (outbox {outboxId}) — may have already been processed")]
    private partial void LogStage3OutboxNotFound(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage3SetupNotFound, Level = LogLevel.Error,
        Message = "Stage3: Problem setup {setupId} not found for submission {submissionId}")]
    private partial void LogStage3SetupNotFound(Guid submissionId, int setupId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage3Completed, Level = LogLevel.Information,
        Message = "Stage3: Evaluation complete for submission {submissionId} (outbox {outboxId})")]
    private partial void LogStage3Completed(Guid submissionId, Guid outboxId);
}