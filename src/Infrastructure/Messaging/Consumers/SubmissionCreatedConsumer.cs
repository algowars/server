using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Consumers;

public sealed class SubmissionCreatedConsumer(IServiceScopeFactory serviceScopeFactory)
    : IConsumer<SubmissionCreatedMessage>
{
    public async Task Consume(ConsumeContext<SubmissionCreatedMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var problemAppService = scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        var codeBuilderService = scope.ServiceProvider.GetRequiredService<ICodeBuilderService>();
        var codeExecutionService = scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults.Value
            .Where(outbox =>
                outbox.Id == context.Message.OutboxId
                && outbox.Type == SubmissionOutboxType.Initialized)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var setupsMap = (
            await problemAppService.GetProblemSetupsForExecutionAsync(
                outboxes.Select(outbox => outbox.Submission.ProblemSetupId),
                cancellationToken
            )
        ).Value.ToDictionary(setup => setup.Id);

        var executionContexts = outboxes
            .Select(outbox =>
            {
                var setup = setupsMap[outbox.Submission.ProblemSetupId];

                var builderContexts = setup
                    .TestSuites.SelectMany(ts => ts.TestCases)
                    .Select(tc => new CodeBuilderContext
                    {
                        Code = outbox.Submission.Code ?? "",
                        Template = setup.HarnessTemplate?.Template ?? "",
                        FunctionName = setup.FunctionName ?? string.Empty,
                        LanguageVersionId = setup.LanguageVersionId,
                        Inputs = tc.Inputs,
                        ExpectedOutput = "",
                    });

                var buildResults = codeBuilderService.Build(builderContexts);

                return new CodeExecutionContext
                {
                    SubmissionId = outbox.SubmissionId,
                    Setup = setup,
                    Code = outbox.Submission.Code ?? "",
                    CreatedById = outbox.Submission.CreatedById,
                    BuiltResults = buildResults.Value,
                };
            })
            .ToList();

        if (executionContexts.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        var submissionResults = await codeExecutionService.ExecuteAsync(
            executionContexts,
            cancellationToken
        );

        await submissionAppService.ProcessSubmissionExecutionAsync(
            submissionResults.Value,
            cancellationToken
        );

        await context.Publish(new SubmissionExecutedMessage
        {
            SubmissionId = context.Message.SubmissionId,
            OutboxId = context.Message.OutboxId,
        }, cancellationToken);
    }
}
