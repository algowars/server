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

        var evaluatedSubmissions = outboxes
            .Where(outbox => setupsMap.ContainsKey(outbox.Submission.ProblemSetupId))
            .Select(outbox =>
            {
                var setup = setupsMap[outbox.Submission.ProblemSetupId];
                var testCases = setup.TestSuites.SelectMany(ts => ts.TestCases);

                var evaluatedResults = reportRunner.EvaluateResults(
                    outbox.Submission.Results,
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
