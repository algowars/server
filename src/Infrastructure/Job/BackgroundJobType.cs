namespace Infrastructure.Job;

public enum BackgroundJobType
{
    SubmissionInitializer = 1,
    SubmissionExecutor,
    SubmissionPoller,
}
