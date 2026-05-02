using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

/// <summary>
/// Recovery sweep for Stage 3: compares each result's stdout against its
/// expected output and transitions outbox Evaluate → EvaluationPoll.
/// Mirrors <see cref="Infrastructure.Messaging.Consumers.SubmissionReadyToEvaluateConsumer"/>.
/// </summary>
[DisallowConcurrentExecution]
public sealed class EvaluateSubmissionHandler(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.EvaluateSubmission;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var problemAppService = scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        var comparisonService = scope.ServiceProvider.GetRequiredService<IExecutionComparisonService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults.Value
            .Where(o => o.Type == SubmissionOutboxType.Evaluate)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var setupIds = outboxes.Select(o => o.Submission.ProblemSetupId).Distinct();
        var setupsMap = (
            await problemAppService.GetProblemSetupsForExecutionAsync(setupIds, cancellationToken)
        ).Value.ToDictionary(s => s.Id);

        var evaluated = new List<SubmissionModel>();

        foreach (var outbox in outboxes)
        {
            if (!setupsMap.TryGetValue(outbox.Submission.ProblemSetupId, out var setup))
            {
                continue;
            }

            var expectedOutputs = setup.TestSuites
                .SelectMany(ts => ts.TestCases)
                .Select(tc => tc.ExpectedOutput.Value)
                .ToList();

            var results = outbox.Submission.Results.ToList();

            for (int i = 0; i < results.Count; i++)
            {
                string expected = i < expectedOutputs.Count ? expectedOutputs[i] : string.Empty;
            results[i].Status = comparisonService.Compare(results[i].ProgramOutput, expected);
            }

            evaluated.Add(new SubmissionModel
            {
                Id = outbox.SubmissionId,
                CreatedById = outbox.Submission.CreatedById,
                Results = results,
            });
        }

        if (evaluated.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(o => o.Id).ToList();
        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);
        await submissionAppService.ProcessEvaluationAsync(evaluated, cancellationToken);
    }
}
