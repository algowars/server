namespace Infrastructure.Job;

public sealed class JobRegistry
{
    private readonly Dictionary<BackgroundJobType, JobRegistration> _jobs = new();

    public void Register<TJob>(BackgroundJobType jobType, TimeSpan interval, bool enabled)
        where TJob : class, IBackgroundJob
    {
        _jobs[jobType] = new JobRegistration(typeof(TJob), interval, enabled);
    }

    public IReadOnlyDictionary<BackgroundJobType, JobRegistration> Jobs => _jobs;
}

public sealed record JobRegistration(Type JobType, TimeSpan Interval, bool Enabled);
