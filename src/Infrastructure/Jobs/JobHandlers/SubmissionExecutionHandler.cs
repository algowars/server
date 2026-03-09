using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs.JobHandlers;

public sealed class SubmissionExecutionHandler(IServiceScopeFactory serviceScopeFactory) : JobBase
{
    public override JobType JobType => JobType.SubmissionExecution;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService =
            scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var problemAppService = scope.ServiceProvider.GetRequiredService<IProblemAppService>();
        var codeBuildingService =
            scope.ServiceProvider.GetRequiredService<ICodeBuildingService>();
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
            outbox.Type == SubmissionOutboxType.Initialized
        );

        var setupsMap = (
            await problemAppService.GetProblemSetupsForExecutionAsync(
                outboxes.Select(outbox => outbox.Submission.ProblemSetupId),
                cancellationToken
            )
        ).Value.ToDictionary(setup => setup.Id);

        var buildResult = codeBuildingService.BuildExecutionContexts(outboxes, setupsMap);

        if (!buildResult.IsSuccess)
        {
            return;
        }

        var executionContexts = buildResult.Value.ToList();

        if (executionContexts.Count == 0)
        {
            return;
        }

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        var submissionResults = await codeExecutionService.ExecuteAsync(
            executionContexts,
            cancellationToken
        );

        await submissionAppService.ProcessSubmissionExecutionAsync(
            submissionResults.Value,
            cancellationToken
        );
    }
}
