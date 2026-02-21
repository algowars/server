using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs;

public sealed class JobRunner(IServiceProvider serviceProvider)
{
    public async Task RunAsync(JobRegistration registration, CancellationToken cancellationToken)
    {
        if (!registration.Enabled)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();

        var job = (IBackgroundJob)scope.ServiceProvider.GetRequiredService(registration.JobType);

        await job.ExecuteAsync(cancellationToken);
    }
}