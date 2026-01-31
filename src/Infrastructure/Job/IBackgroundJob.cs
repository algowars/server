namespace Infrastructure.Job;

public interface IBackgroundJob
{
    BackgroundJobType JobType { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
