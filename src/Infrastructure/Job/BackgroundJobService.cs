using ApplicationCore.Domain.Job;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Job;

public sealed class BackgroundJobService(
    JobRegistry registry,
    JobRunner runner,
    ILogger<BackgroundJobService> logger
) : BackgroundService
{
    private readonly ILogger<BackgroundJobService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastRun = new Dictionary<BackgroundJobType, DateTimeOffset>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var job in registry.Jobs)
            {
                if (
                    lastRun.TryGetValue(job.JobType, out var previous)
                    && now - previous < job.Interval
                )
                {
                    continue;
                }

                lastRun[job.JobType] = now;
                await runner.RunAsync(job, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
