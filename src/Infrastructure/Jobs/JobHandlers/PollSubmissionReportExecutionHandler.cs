using ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure.Jobs.JobHandlers;

[DisallowConcurrentExecution]
internal sealed class PollSubmissionReportExecutionHandler(IServiceScopeFactory serviceScopeFactory)
    : JobBase
{
    public override JobType JobType => JobType.PollSubmissionReportExecution;

    protected override async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var submissionAppService =
            scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var codeExecutionService =
            scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();
    }
}
