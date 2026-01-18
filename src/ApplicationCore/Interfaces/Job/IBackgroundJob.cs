using ApplicationCore.Domain.Job;

namespace ApplicationCore.Interfaces.Job;

public interface IBackgroundJob
{
    BackgroundJobType JobType { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
