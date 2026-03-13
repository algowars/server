namespace ApplicationCore.Domain.Submissions.Outboxes;

public enum SubmissionOutboxType
{
    Initialized = 1,
    ExecuteSubmission,
    PollExecution,
    EvaluateSubmission,
    PollEvaluation,
}
