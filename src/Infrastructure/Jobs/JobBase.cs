using Quartz;

namespace Infrastructure.Jobs;

public abstract class JobBase : IJob
{
    public abstract JobType JobType { get; }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await ExecuteJobAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }

    protected abstract Task ExecuteJobAsync(CancellationToken cancellationToken);
}