using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Logging;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Stage 1: Build submission code and send to Judge0.
/// Stores the Judge0 execution tokens (ExecutionId) on each SubmissionResult,
/// transitions the outbox Initialized to PollExecution, then publishes
/// SubmissionExecutionPollMessage to kick off polling.
/// </summary>
public sealed partial class SubmissionCreatedConsumer(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SubmissionCreatedConsumer> logger
) : IConsumer<SubmissionCreatedMessage>
{
    private readonly ILogger<SubmissionCreatedConsumer> _logger = logger;
    public async Task Consume(ConsumeContext<SubmissionCreatedMessage> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        LogStage1Started(context.Message.SubmissionId, context.Message.OutboxId);

        ISubmissionAppService submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        IProblemAppService problemAppService = scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        ICodeBuilderService codeBuilderService = scope.ServiceProvider.GetRequiredService<ICodeBuilderService>();
        ICodeExecutionService codeExecutionService = scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        Ardalis.Result.Result<System.Collections.Generic.IEnumerable<SubmissionOutboxModel>> outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            LogStage1OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        SubmissionOutboxModel? outbox = outboxResults.Value.FirstOrDefault(o =>
            o.Id == context.Message.OutboxId
            && o.Type == SubmissionOutboxType.Initialized);

        if (outbox is null)
        {
            LogStage1OutboxNotFound(context.Message.SubmissionId, context.Message.OutboxId);
            return;
        }

        Ardalis.Result.Result<IEnumerable<ApplicationCore.Domain.Problems.ProblemSetups.ProblemSetupModel>> setupResult = await problemAppService.GetProblemSetupsForExecutionAsync(
            [outbox.Submission.ProblemSetupId],
            cancellationToken
        );

        if (!setupResult.IsSuccess)
        {
            LogStage1SetupFailed(context.Message.SubmissionId, outbox.Submission.ProblemSetupId, string.Join(", ", setupResult.Errors));
            return;
        }

        ApplicationCore.Domain.Problems.ProblemSetups.ProblemSetupModel? setup = setupResult.Value.FirstOrDefault(s => s.Id == outbox.Submission.ProblemSetupId);

        if (setup is null)
        {
            LogStage1SetupFailed(context.Message.SubmissionId, outbox.Submission.ProblemSetupId, "Setup not found after query");
            return;
        }

        System.Collections.Generic.IEnumerable<CodeBuilderContext> builderContexts = setup.TestSuites
            .SelectMany(ts => ts.TestCases)
            .Select(tc => new CodeBuilderContext
            {
                Code = outbox.Submission.Code ?? "",
                Template = setup.HarnessTemplate?.Template ?? "",
                FunctionName = setup.FunctionName ?? string.Empty,
                LanguageVersionId = setup.LanguageVersionId,
                Judge0LanguageId = setup.LanguageVersion?.Judge0LanguageId,
                Inputs = tc.Inputs,
                ExpectedOutput = tc.ExpectedOutput,
            })
            .ToList();

        if (!builderContexts.Any())
        {
            LogStage1BuildFailed(context.Message.SubmissionId, setup.Id, "No test cases found for setup");
            return;
        }

        Ardalis.Result.Result<System.Collections.Generic.IEnumerable<CodeBuildResult>> buildResult = codeBuilderService.Build(builderContexts);

        if (!buildResult.IsSuccess)
        {
            LogStage1BuildFailed(context.Message.SubmissionId, setup.Id, string.Join(", ", buildResult.Errors));
            return;
        }

        CodeExecutionContext executionContext = new()
        {
            SubmissionId = outbox.SubmissionId,
            Setup = setup,
            Code = outbox.Submission.Code ?? "",
            CreatedById = outbox.Submission.CreatedById,
            BuiltResults = buildResult.Value,
        };

        DateTime now = DateTime.UtcNow;
        await submissionAppService.IncrementOutboxesCountAsync([outbox.Id], now, cancellationToken);

        Ardalis.Result.Result<System.Collections.Generic.IEnumerable<ApplicationCore.Domain.Submissions.SubmissionModel>> executeResult = await codeExecutionService.ExecuteAsync(
            [executionContext],
            cancellationToken
        );

        if (!executeResult.IsSuccess)
        {
            LogStage1ExecutionFailed(context.Message.SubmissionId, string.Join(", ", executeResult.Errors));
            return;
        }

        await submissionAppService.SaveExecutionTokensAsync(executeResult.Value, cancellationToken);

        await context.Publish(new SubmissionExecutionPollMessage
        {
            SubmissionId = context.Message.SubmissionId,
            OutboxId = context.Message.OutboxId,
        }, cancellationToken);

        LogStage1Completed(context.Message.SubmissionId, context.Message.OutboxId);
    }

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage1Started, Level = LogLevel.Information,
        Message = "Stage1: Starting submission execution for {submissionId} (outbox {outboxId})")]
    private partial void LogStage1Started(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage1OutboxNotFound, Level = LogLevel.Warning,
        Message = "Stage1: Outbox not found or not in Initialized state for submission {submissionId} (outbox {outboxId}) — may have already been processed")]
    private partial void LogStage1OutboxNotFound(Guid submissionId, Guid outboxId);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage1SetupFailed, Level = LogLevel.Error,
        Message = "Stage1: Failed to get problem setup {setupId} for submission {submissionId}: {errors}")]
    private partial void LogStage1SetupFailed(Guid submissionId, int setupId, string errors);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage1BuildFailed, Level = LogLevel.Error,
        Message = "Stage1: Code build failed for submission {submissionId} (setup {setupId}): {errors}")]
    private partial void LogStage1BuildFailed(Guid submissionId, int setupId, string errors);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage1ExecutionFailed, Level = LogLevel.Error,
        Message = "Stage1: Execution failed for submission {submissionId}: {errors}")]
    private partial void LogStage1ExecutionFailed(Guid submissionId, string errors);

    [LoggerMessage(EventId = LoggingEventIds.Submissions.Stage1Completed, Level = LogLevel.Information,
        Message = "Stage1: Completed submission execution for {submissionId} (outbox {outboxId})")]
    private partial void LogStage1Completed(Guid submissionId, Guid outboxId);
}