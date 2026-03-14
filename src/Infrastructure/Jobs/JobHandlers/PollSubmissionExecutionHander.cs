using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs.JobHandlers;

internal sealed class PollSubmissionExecutionHander(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.PollSubmissionExecution;
    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var codeExecutionService = scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults.Value.Where(outbox => 
            outbox.Type == SubmissionOutboxType.PollExecution).ToList();

        var submissionResults =
            await codeExecutionService.GetSubmissionResultsAsync(outboxes.Select(o => 
                o.Submission), cancellationToken
                );

        await submissionAppService.ProcessPollingSubmissionExecutionsAsync(submissionResults.Value, cancellationToken);
    }
}