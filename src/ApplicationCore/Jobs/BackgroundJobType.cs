namespace ApplicationCore.Jobs;

public enum BackgroundJobType
{
    SubmissionExecution = 1,
    SubmissionEvaluator,
    SubmissionPoller,
}
