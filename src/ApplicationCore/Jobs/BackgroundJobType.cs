namespace ApplicationCore.Jobs;

public enum BackgroundJobType
{
    SubmissionInitializer = 1,
    SubmissionExecutor,
    SubmissionPoller,
}
