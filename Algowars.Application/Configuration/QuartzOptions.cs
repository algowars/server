namespace Algowars.Application.Configuration;

using Algowars.Application.Settings;

public sealed class JobScheduleOptions : IOption
{
    public static string SectionName => "Quartz";

    public SubmissionCleanupJobOptions SubmissionCleanupJob { get; init; } = new();
}

public sealed class SubmissionCleanupJobOptions
{
    /// <summary>
    /// Quartz cron expression for the scheduled run. Defaults to the top of every hour.
    /// </summary>
    public string CronExpression { get; init; } = "0 0 * * * ?";
}
