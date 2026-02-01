using ApplicationCore.Jobs;

namespace Infrastructure.Jobs;

public interface IBackgroundJob
{
    BackgroundJobType JobType { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
