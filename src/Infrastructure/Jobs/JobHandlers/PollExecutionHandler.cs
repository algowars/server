using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs.JobHandlers;

public sealed class PollExecutionHandler(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.PollExecution;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService =
            scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var codeExecutionService =
            scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(
            cancellationToken
        );

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults.Value.Where(outbox =>
            outbox.Type == SubmissionOutboxType.PollEvaluation
        );

        if (!outboxes.Any())
        {
            return;
        }

        var submissionResults = await codeExecutionService.GetSubmissionResultsAsync(
            outboxes.Select(outbox => outbox.Submission),
            cancellationToken
        );

        await submissionAppService.ProcessPollExecutionAsync(
            submissionResults.Value,
            cancellationToken
        );
    }
}
