using ApplicationCore.Interfaces.Job;

namespace ApplicationCore.Domain.Job;

public sealed class JobRegistry
{
    private readonly Dictionary<BackgroundJobType, JobDescriptor> _jobs = new();

    public IReadOnlyCollection<JobDescriptor> Jobs => _jobs.Values;

    public void Register<TJob>(BackgroundJobType jobType, TimeSpan interval, bool enabled = true)
        where TJob : IBackgroundJob
    {
        if (_jobs.ContainsKey(jobType))
        {
            throw new InvalidOperationException($"Job '{jobType}' is already registered.");
        }

        _jobs[jobType] = new JobDescriptor
        {
            JobType = jobType,
            Interval = interval,
            Enabled = enabled,
            ImplementationType = typeof(TJob),
        };
    }
}
