using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Jobs;

namespace Infrastructure.Jobs.JobHandlers;

public sealed class SubmissionInitializerHandler(
    ISubmissionAppService submissionAppService,
    IProblemAppService problemAppService,
    ICodeBuilderService codeBuilderService,
    ISubmissionRepository submissionRepository,
    ICodeExecutionService codeExecutionService
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionInitializer;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var outboxesResult = await submissionAppService.GetSubmissionOutboxesAsync(
            cancellationToken
        );

        if (!outboxesResult.IsSuccess || !outboxesResult.Value.Any())
        {
            return;
        }

        var outboxes = outboxesResult
            .Value.Where(outbox => outbox.Type == SubmissionOutboxType.Initialized)
            .ToList();

        var setupsMap = (
            await problemAppService.GetProblemSetupsForExecutionAsync(
                outboxes.Select(s => s.Submission.ProblemSetupId),
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
                        Code = outbox.Submission.Code,
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
                    Code = outbox.Submission.Code,
                    CreatedById = outbox.Submission.CreatedById,
                    BuiltResults = buildResults.Value,
                };
            })
            .ToList();

        if (executionContexts.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(o => o.Id).ToList();
        var now = DateTime.UtcNow;
        await submissionRepository.IncrementOutboxesCount(outboxIds, now, cancellationToken);

        var submissionsResult = await codeExecutionService.ExecuteAsync(
            executionContexts,
            cancellationToken
        );

        await submissionRepository.ProcessSubmissionInitialization(
            submissionsResult.Value,
            cancellationToken
        );
    }
}
