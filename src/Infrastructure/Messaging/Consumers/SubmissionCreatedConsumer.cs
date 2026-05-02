using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Stage 1: Build submission code and send to Judge0.
/// Stores the Judge0 execution tokens (ExecutionId) on each SubmissionResult,
/// transitions the outbox Initialized to PollExecution, then publishes
/// SubmissionExecutionPollMessage to kick off polling.
/// </summary>
public sealed class SubmissionCreatedConsumer(IServiceScopeFactory serviceScopeFactory)
    : IConsumer<SubmissionCreatedMessage>
{
    public async Task Consume(ConsumeContext<SubmissionCreatedMessage> context)
    {
        CancellationToken cancellationToken = context.CancellationToken;
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        ISubmissionAppService submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        IProblemAppService problemAppService = scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        ICodeBuilderService codeBuilderService = scope.ServiceProvider.GetRequiredService<ICodeBuilderService>();
        ICodeExecutionService codeExecutionService = scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        Ardalis.Result.Result<System.Collections.Generic.IEnumerable<SubmissionOutboxModel>> outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        SubmissionOutboxModel? outbox = outboxResults.Value.FirstOrDefault(o =>
            o.Id == context.Message.OutboxId
            && o.Type == SubmissionOutboxType.Initialized);

        if (outbox is null)
        {
            return;
        }

        Ardalis.Result.Result<IEnumerable<ApplicationCore.Domain.Problems.ProblemSetups.ProblemSetupModel>> setupResult = await problemAppService.GetProblemSetupsForExecutionAsync(
            [outbox.Submission.ProblemSetupId],
            cancellationToken
        );

        if (!setupResult.IsSuccess)
        {
            return;
        }

        ApplicationCore.Domain.Problems.ProblemSetups.ProblemSetupModel? setup = setupResult.Value.FirstOrDefault(s => s.Id == outbox.Submission.ProblemSetupId);

        if (setup is null)
        {
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
                Inputs = tc.Inputs,
                ExpectedOutput = tc.ExpectedOutput,
            });

        Ardalis.Result.Result<System.Collections.Generic.IEnumerable<CodeBuildResult>> buildResult = codeBuilderService.Build(builderContexts);

        if (!buildResult.IsSuccess)
        {
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
            return;
        }

        await submissionAppService.SaveExecutionTokensAsync(executeResult.Value, cancellationToken);

        await context.Publish(new SubmissionExecutionPollMessage
        {
            SubmissionId = context.Message.SubmissionId,
            OutboxId = context.Message.OutboxId,
        }, cancellationToken);
    }
}