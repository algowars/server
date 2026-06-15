namespace Algowars.Domain.Submissions.Outbox.Enums;

public enum SubmissionOutboxStatus
{
    Pending = 1,
    Processing = 2,
    Retrying = 3,
    Completed = 4,
    Failed = 5,
    Abandoned = 6
}
