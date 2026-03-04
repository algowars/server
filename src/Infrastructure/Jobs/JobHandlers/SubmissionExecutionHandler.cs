using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Jobs;

namespace Infrastructure.Jobs.JobHandlers;

public sealed class SubmissionExecutionHandler(
    ISubmissionAppService submissionAppService,
    IProblemAppService problemAppService,
    ICodeBuilderService codeBuilderService,
    ICodeExecutionService codeExecutionService
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionExecution;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(
            cancellationToken
        );

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults.Value.Where(outbox =>
            outbox.Type == SubmissionOutboxType.Initialized
        );

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
                        Inputs = tc.Input,
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
    }
}
