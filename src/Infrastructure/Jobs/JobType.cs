namespace Infrastructure.Jobs;

public enum JobType
{
    SubmissionExecution = 1,
    SubmissionEvaluator,
    SubmissionPoller,
    PollSubmissionExecution,
}
