using ApplicationCore.Jobs;

namespace Infrastructure.Jobs;

public sealed class JobDescriptor
{
    public required BackgroundJobType JobType { get; init; }
    public required TimeSpan Interval { get; init; }
    public bool Enabled { get; init; } = true;
    public required Type ImplementationType { get; init; }
}
