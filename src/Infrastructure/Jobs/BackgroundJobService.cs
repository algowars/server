using ApplicationCore.Jobs;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Jobs;

public sealed class BackgroundJobService(JobRegistry registry, JobRunner runner) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastRun = new Dictionary<BackgroundJobType, DateTimeOffset>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var (jobType, registration) in registry.Jobs)
            {
                if (!registration.Enabled)
                {
                    continue;
                }

                if (
                    lastRun.TryGetValue(jobType, out var previous)
                    && now - previous < registration.Interval
                )
                {
                    continue;
                }

                lastRun[jobType] = now;

                await runner.RunAsync(registration, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}