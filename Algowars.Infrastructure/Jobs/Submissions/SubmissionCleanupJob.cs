using Algowars.Application.Jobs.Submissions;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Algowars.Infrastructure.Jobs.Submissions;

[DisallowConcurrentExecution]
internal sealed partial class SubmissionCleanupJob(
    ISubmissionCleanupService cleanupService,
    ILogger<SubmissionCleanupJob> logger
) : IJob
{
    public static readonly JobKey Key = new(nameof(SubmissionCleanupJob), "Submissions");

    public async Task Execute(IJobExecutionContext context)
    {
        LogExecuting();
        try
        {
            await cleanupService.RunAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            LogFailed(ex);
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Executing SubmissionCleanupJob")]
    private partial void LogExecuting();

    [LoggerMessage(Level = LogLevel.Error, Message = "SubmissionCleanupJob failed")]
    private partial void LogFailed(Exception ex);
}