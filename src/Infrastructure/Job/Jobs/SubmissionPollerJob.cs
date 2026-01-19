using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionPollerJob(
    ISubmissionAppService submissionAppService,
    IProblemAppService problemAppService,
    ICodeBuilderService codeBuilderService,
    ICodeExecutionService codeExecutionService,
    ISubmissionRepository submissionRepository
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionPoller;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var submissionOutboxResult = await submissionAppService.GetOutboxesAsync(cancellationToken);

        if (!submissionOutboxResult.IsSuccess)
        {
            return;
        }

        var submissionsThatNeedExecution = submissionOutboxResult
            .Value.Where(outbox => outbox.Type == SubmissionOutboxType.ExecuteSubmission)
            .ToList();

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

        await submissionRepository.BulkUpsertAsync(results.Value, cancellationToken);

        var submissionOutboxesThatNeedToPoll = submissionOutboxResult.Value.Where(s =>
            s.Type == SubmissionOutboxType.PollJudge0Result
        );
    }
}
