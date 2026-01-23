using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionExecutorJobOld(
    ISubmissionAppService submissionAppService,
    IProblemAppService problemAppService,
    ICodeBuilderService codeBuilderService,
    ICodeExecutionService codeExecutionService,
    ISubmissionRepository submissionRepository
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionExecutor;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var submissionOutboxResult = await submissionAppService.GetOutboxesAsync(cancellationToken);

        var submissionsThatNeedExecution = submissionOutboxResult
            .Value.Where(outbox => outbox.Type == SubmissionOutboxType.ExecuteSubmission)
            .ToList();
        
        foreach (var submissionOutboxModel in submissionsThatNeedExecution)
        {
            submissionOutboxModel.
        }

        var setupsResult = await problemAppService.GetProblemSetupsForExecutionAsync(
            submissionsThatNeedExecution.Select(s => s.Submission.ProblemSetupId),
            cancellationToken
        );

        var setupsMap = setupsResult.Value.ToDictionary(setup => setup.Id);

        var executionContexts = submissionsThatNeedExecution.Select(submissionOutbox =>
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

        var results = await codeExecutionService.ExecuteAsync(executionContexts, cancellationToken);

        await submissionRepository.BulkUpsertResultsAsync(results.Value, cancellationToken);

        var now = DateTime.UtcNow;

        var executedOutboxIds = submissionsThatNeedExecution.Select(o => o.Id).ToList();

        await submissionRepository.MarkOutboxesAsPollingAsync(
            executedOutboxIds,
            now,
            cancellationToken
        );
    }
}
