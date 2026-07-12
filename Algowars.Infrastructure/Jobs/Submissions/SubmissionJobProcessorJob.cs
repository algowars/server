using Microsoft.Extensions.Logging;
using Quartz;

namespace Algowars.Infrastructure.Jobs.Submissions;

[DisallowConcurrentExecution]
internal sealed partial class SubmissionJobProcessorJob(
    SubmissionJobProcessorService processorService,
    ILogger<SubmissionJobProcessorJob> logger) : IJob
{
    public static readonly JobKey Key = new(nameof(SubmissionJobProcessorJob), "Submissions");

    public async Task Execute(IJobExecutionContext context)
    {
        LogExecuting();
        try
        {
            await processorService.RunAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            LogFailed(ex);
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Executing SubmissionJobProcessorJob")]
    private partial void LogExecuting();

    [LoggerMessage(Level = LogLevel.Error, Message = "SubmissionJobProcessorJob failed")]
    private partial void LogFailed(Exception ex);
}
