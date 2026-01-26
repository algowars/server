using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionExecutorHandler(
    IProblemAppService problemAppService,
    ICodeBuilderService codeBuilderService,
    ISubmissionRepository submissionRepository,
    ICodeExecutionService codeExecutionService
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionExecutor;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var outboxSubmissions = await submissionRepository.GetSubmissionExecutionOutboxesAsync(
            cancellationToken
        );

        var setupsMap = (
            await problemAppService.GetProblemSetupsForExecutionAsync(
                outboxSubmissions.Select(s => s.Submission.ProblemSetupId),
                cancellationToken
            )
        ).Value.ToDictionary(setup => setup.Id);

        var executionContexts = outboxSubmissions.Select(submissionOutbox =>
        {
            var setup = setupsMap[submissionOutbox.Submission.ProblemSetupId];

            var builderContexts = setup
                .TestSuites.SelectMany(ts => ts.TestCases)
                .Select(tc => new CodeBuilderContext
                {
                    InitialCode = setup.InitialCode,
                    Template = setup.HarnessTemplate.Template,
                    FunctionName = setup.FunctionName ?? string.Empty,
                    LanguageVersionId = setup.LanguageVersionId,
                    Inputs = tc.Input,
                    ExpectedOutput = tc.ExpectedOutput,
                });

            var buildResults = codeBuilderService.Build(builderContexts);

            return new CodeExecutionContext
            {
                SubmissionId = submissionOutbox.SubmissionId,
                Setup = setup,
                Code = submissionOutbox.Submission.Code,
                CreatedById = submissionOutbox.Submission.CreatedById,
                BuiltResults = buildResults.Value,
            };
        });

        if (!executionContexts.Any())
        {
            return;
        }

        var outboxIds = outboxSubmissions.Select(o => o.Id).ToList();
        var now = DateTime.UtcNow;
        await submissionRepository.IncrementOutboxesCount(outboxIds, now, cancellationToken);

        var submissionsResult = await codeExecutionService.ExecuteAsync(
            executionContexts,
            cancellationToken
        );

        await submissionRepository.ProcessSubmissionExecution(
            submissionsResult.Value,
            cancellationToken
        );
    }
}
