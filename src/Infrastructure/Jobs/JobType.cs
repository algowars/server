namespace Infrastructure.Jobs;

public enum JobType
{
    SubmissionExecution = 1,
    PollExecution,
    SubmissionEvaluator,
    PollEvaluation,
}
