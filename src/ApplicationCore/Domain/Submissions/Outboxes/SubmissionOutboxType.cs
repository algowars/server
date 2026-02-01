namespace ApplicationCore.Domain.Submissions.Outboxes;

public enum SubmissionOutboxType
{
    Initialized = 1,
    PollInitialization,
    ExecuteSubmission,
    PollJudge0Result,
}
