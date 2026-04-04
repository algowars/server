using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

[DisallowConcurrentExecution]
internal sealed class SubmissionReportExecutionHandler(IServiceScopeFactory serviceScopeFactory)
    : JobBase
{
    public override JobType JobType => JobType.SubmissionReportExecution;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService =
            scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var problemAppService =
            scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        var codeBuilderService =
            scope.ServiceProvider.GetRequiredService<ICodeBuilderService>();
        var codeExecutionService =
            scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();
        var reportRunner =
            scope.ServiceProvider.GetRequiredService<ISubmissionReportRunner>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(
            cancellationToken
        );

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults
            .Value.Where(outbox => outbox.Type == SubmissionOutboxType.Evaluate)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var setupIds = outboxes
            .Select(o => o.Submission.ProblemSetupId)
            .Distinct();

        var setupsResult = await problemAppService.GetProblemSetupsForExecutionAsync(
            setupIds,
            cancellationToken
        );

        if (!setupsResult.IsSuccess)
        {
            return;
        }

        var setupsMap = setupsResult.Value.ToDictionary(s => s.Id);

        var validOutboxes = outboxes
            .Where(outbox => setupsMap.ContainsKey(outbox.Submission.ProblemSetupId))
            .ToList();

        if (validOutboxes.Count == 0)
        {
            return;
        }

        var executionContexts = validOutboxes
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
                        ExpectedOutput = tc.ExpectedOutput,
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

        var executionResults = await codeExecutionService.ExecuteAsync(
            executionContexts,
            cancellationToken
        );

        if (!executionResults.IsSuccess)
        {
            return;
        }

        var executionMap = executionResults.Value.ToDictionary(s => s.Id);

        var evaluatedSubmissions = validOutboxes
            .Where(outbox => executionMap.ContainsKey(outbox.Submission.Id))
            .Select(outbox =>
            {
                var setup = setupsMap[outbox.Submission.ProblemSetupId];
                var testCases = setup.TestSuites.SelectMany(ts => ts.TestCases);

                var existingResults = outbox.Submission.Results.ToList();
                var newResults = executionMap[outbox.Submission.Id].Results.ToList();

                for (int i = 0; i < existingResults.Count && i < newResults.Count; i++)
                {
                    existingResults[i].ResultId = newResults[i].ExecutionId;
                    existingResults[i].ExpectedOutput = newResults[i].ExpectedOutput;
                }

                var evaluatedResults = reportRunner.EvaluateResults(
                    existingResults,
                    testCases
                );

                return new ApplicationCore.Domain.Submissions.SubmissionModel
                {
                    Id = outbox.Submission.Id,
                    ProblemSetupId = outbox.Submission.ProblemSetupId,
                    Code = outbox.Submission.Code,
                    CreatedOn = outbox.Submission.CreatedOn,
                    CreatedById = outbox.Submission.CreatedById,
                    Results = evaluatedResults,
                };
            })
            .ToList();

        if (evaluatedSubmissions.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;
        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        await submissionAppService.ProcessSubmissionReportExecutionAsync(
            evaluatedSubmissions,
            cancellationToken
        );
    }
}
