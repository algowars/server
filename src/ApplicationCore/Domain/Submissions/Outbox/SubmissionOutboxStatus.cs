namespace ApplicationCore.Domain.Submissions.Outbox;

public enum SubmissionOutboxStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
}
