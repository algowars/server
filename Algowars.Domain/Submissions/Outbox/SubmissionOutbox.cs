using Algowars.Domain.SeedWork;
using Algowars.Domain.Submissions.Outbox.Enums;

namespace Algowars.Domain.Submissions.Outbox;

public sealed class SubmissionOutbox : AggregateRoot
{
    private SubmissionOutbox() { }

    private SubmissionOutbox(Guid submissionId, SubmissionOutboxStep step, int maxAttempts)
    {
        SubmissionId = submissionId;
        Step = step;
        Status = SubmissionOutboxStatus.Pending;
        AttemptCount = 0;
        MaxAttempts = maxAttempts;
        CreatedAt = DateTime.UtcNow;
    }

    public static SubmissionOutbox CreateForStep(
        Guid submissionId,
        SubmissionOutboxStep step,
        int maxAttempts = 5)
        => new(submissionId, step, maxAttempts);

    public static SubmissionOutbox Reconstitute(
        Guid id,
        Guid submissionId,
        SubmissionOutboxStep step,
        SubmissionOutboxStatus status,
        int attemptCount,
        int maxAttempts,
        DateTime createdAt,
        DateTime? lastAttemptAt,
        DateTime? completedAt,
        string? lastError)
    {
        var outbox = new SubmissionOutbox
        {
            SubmissionId = submissionId,
            Step = step,
            Status = status,
            AttemptCount = attemptCount,
            MaxAttempts = maxAttempts,
            CreatedAt = createdAt,
            LastAttemptAt = lastAttemptAt,
            CompletedAt = completedAt,
            LastError = lastError,
        };
        outbox.Id = id;
        return outbox;
    }

    public void RecordAttempt(DateTime now)
    {
        AttemptCount++;
        Status = SubmissionOutboxStatus.Processing;
        LastAttemptAt = now;
    }

    public void Complete(DateTime now)
    {
        Status = SubmissionOutboxStatus.Completed;
        CompletedAt = now;
    }

    public void RecordFailure(string error, DateTime now)
    {
        LastError = error;
        LastAttemptAt = now;
        Status = CanRetry ? SubmissionOutboxStatus.Retrying : SubmissionOutboxStatus.Failed;
    }

    public bool CanRetry => AttemptCount < MaxAttempts;

    public Guid SubmissionId { get; private set; }
    public SubmissionOutboxStep Step { get; private set; }
    public SubmissionOutboxStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? LastError { get; private set; }
}
