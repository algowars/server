namespace Infrastructure.Jobs;

public enum JobType
{
    SubmissionExecution = 1,
    PollSubmissionExecution,
    EvaluateSubmission,
    PollEvaluation,
}