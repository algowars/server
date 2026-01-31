using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Job;

public sealed class BackgroundJobService(
    JobRegistry registry,
    JobRunner runner,
    ILogger<BackgroundJobService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastRun = new Dictionary<BackgroundJobType, DateTimeOffset>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var entry in registry.Jobs)
            {
                var jobType = entry.Key;
                var registration = entry.Value;

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
