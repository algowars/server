using Algowars.Application.Jobs.Submissions;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.Jobs.Submissions;

internal sealed partial class SubmissionCleanupService(
    ILogger<SubmissionCleanupService> logger
) : ISubmissionCleanupService
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        LogRunning();

        await Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Running submission cleanup")]
    private partial void LogRunning();
}
