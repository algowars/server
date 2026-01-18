namespace ApplicationCore.Domain.Submissions.Outbox;

public enum SubmissionOutboxType
{
    Initialized = 1,
    ExecuteSubmission = 2,
    PollJudge0Result = 3,
}
