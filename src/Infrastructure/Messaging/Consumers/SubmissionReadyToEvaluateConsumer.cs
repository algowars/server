using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Stage 3: Compare each result's stdout against its expected output using
/// <see cref="IExecutionComparisonService"/>. Persists Accepted/WrongAnswer
/// statuses, transitions outbox Evaluate → EvaluationPoll, then publishes
/// <see cref="SubmissionEvaluationPollMessage"/>.
/// </summary>
public sealed class SubmissionReadyToEvaluateConsumer(IServiceScopeFactory serviceScopeFactory)
    : IConsumer<SubmissionReadyToEvaluateMessage>
{
    public async Task Consume(ConsumeContext<SubmissionReadyToEvaluateMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var problemAppService = scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        var comparisonService = scope.ServiceProvider.GetRequiredService<IExecutionComparisonService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outbox = outboxResults.Value.FirstOrDefault(o =>
            o.SubmissionId == context.Message.SubmissionId
            && o.Type == SubmissionOutboxType.Evaluate);

        if (outbox is null)
        {
            return;
        }

        // Reload the problem setup to get expected outputs per test case
        var setupResult = await problemAppService.GetProblemSetupsForExecutionAsync(
            [outbox.Submission.ProblemSetupId],
            cancellationToken
        );

        if (!setupResult.IsSuccess)
        {
            return;
        }

        var setup = setupResult.Value.FirstOrDefault(s => s.Id == outbox.Submission.ProblemSetupId);

        if (setup is null)
        {
            return;
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

        var evaluated = new SubmissionModel
        {
            Id = outbox.SubmissionId,
            CreatedById = outbox.Submission.CreatedById,
            Results = results,
        };

        var now = DateTime.UtcNow;
        await submissionAppService.IncrementOutboxesCountAsync([outbox.Id], now, cancellationToken);

        // Persist comparison results and transition Evaluate → EvaluationPoll
        await submissionAppService.ProcessEvaluationAsync([evaluated], cancellationToken);

        await context.Publish(new SubmissionEvaluationPollMessage
        {
            SubmissionId = context.Message.SubmissionId,
            OutboxId = context.Message.OutboxId,
        }, cancellationToken);
    }
}