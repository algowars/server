namespace ApplicationCore.Domain.Submissions.Outboxes;

public enum SubmissionOutboxType
{
    Initialized = 1,
    Execute,
    PollExecution,
    Evaluate
}