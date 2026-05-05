using ApplicationCore.Logging;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Jobs;

public abstract partial class JobBase : IJob
{
    public abstract JobType JobType { get; }

    protected abstract ILogger Logger { get; }

    public async Task Execute(IJobExecutionContext context)
    {
        LogJobStarted(Logger, JobType);
        try
        {
            await ExecuteJobAsync(context.CancellationToken);
            LogJobCompleted(Logger, JobType);
        }
        catch (Exception ex)
        {
            LogJobFailed(Logger, JobType, ex);
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    protected abstract Task ExecuteJobAsync(CancellationToken cancellationToken);

    [LoggerMessage(
        EventId = LoggingEventIds.Jobs.Started,
        Level = LogLevel.Information,
        Message = "Job {jobType} started"
    )]
    private static partial void LogJobStarted(ILogger logger, JobType jobType);

    [LoggerMessage(
        EventId = LoggingEventIds.Jobs.Completed,
        Level = LogLevel.Information,
        Message = "Job {jobType} completed"
    )]
    private static partial void LogJobCompleted(ILogger logger, JobType jobType);

    [LoggerMessage(
        EventId = LoggingEventIds.Jobs.Failed,
        Level = LogLevel.Error,
        Message = "Job {jobType} failed"
    )]
    private static partial void LogJobFailed(ILogger logger, JobType jobType, Exception ex);
}
