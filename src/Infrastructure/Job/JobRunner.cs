using ApplicationCore.Domain.Job;
using ApplicationCore.Interfaces.Job;

namespace Infrastructure.Job;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public sealed class JobRunner(IServiceProvider serviceProvider, ILogger<JobRunner> logger)
{
    public async Task RunAsync(JobDescriptor descriptor, CancellationToken cancellationToken)
    {
        if (!descriptor.Enabled)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();

        var job = (IBackgroundJob)
            scope.ServiceProvider.GetRequiredService(descriptor.ImplementationType);

        try
        {
            logger.LogInformation("Running job {JobType}", descriptor.JobType);

            await job.ExecuteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job {JobType} failed", descriptor.JobType);
        }
    }
}
