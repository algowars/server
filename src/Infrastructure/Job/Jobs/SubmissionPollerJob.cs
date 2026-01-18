using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Job;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Job;
using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Job.Jobs;

public sealed class SubmissionPollerJob(
    ISubmissionAppService submissionAppService,
    IProblemAppService problemAppService,
    ICodeBuilderService codeBuilderService,
    ICodeExecutionService codeExecutionService
) : IBackgroundJob
{
    public BackgroundJobType JobType => BackgroundJobType.SubmissionPoller;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var submissionOutboxResult = await submissionAppService.GetOutboxesAsync(cancellationToken);

        if (submissionOutboxResult.IsSuccess)
        {
            var submissionsThatNeedExecution = submissionOutboxResult.Value.Where(outbox =>
                outbox.Type is SubmissionOutboxType.ExecuteSubmission
            );

            var setupsResult = await problemAppService.GetProblemSetupsForExecutionAsync(
                submissionsThatNeedExecution.Select(s => s.Submission.ProblemSetupId),
                cancellationToken
            );

            var setupsMap = setupsResult.Value.ToDictionary(setup => setup.Id);

            var executionContexts = submissionsThatNeedExecution.Select(submissionOutbox =>
            {
                var setup = setupsMap[submissionOutbox.Submission.ProblemSetupId];

                return (
                    new CodeExecutionContext
                    {
                        Setup = setup,
                        Code = submissionOutbox.Submission.Code,
                        BuiltResults = [],
                        CreatedById = submissionOutbox.Submission.CreatedById,
                    },
                    setup.TestSuites.Select(ts =>
                        ts.TestCases.Select(tc => new CodeBuilderContext
                        {
                            InitialCode = setup.InitialCode,
                            Template = setup.HarnessTemplate.Template,
                            FunctionName = setup.FunctionName,
                            LanguageVersionId = setup.LanguageVersionId,
                            Inputs = tc.Input,
                            ExpectedOutput = tc.ExpectedOutput,
                        })
                    )
                );
            });

            var formattedContexts = executionContexts.Select(
                (executionContext, codeBuilderContext) =>
                {
                    executionContext.BuiltResults = codeBuilderContext.Select(context =>
                        codeBuilderService.BuildAsync(context)
                    );
                }
            );
        }
    }
}
