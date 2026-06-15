using Algowars.Domain.Submissions.Outbox.Enums;

namespace Algowars.Domain.Submissions.Outbox;

public interface ISubmissionOutboxRepository
{
    Task AddAsync(SubmissionOutbox outbox, CancellationToken cancellationToken);
    Task UpdateAsync(SubmissionOutbox outbox, CancellationToken cancellationToken);
    Task<SubmissionOutbox?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubmissionOutbox>> GetPendingByStepAsync(SubmissionOutboxStep step, int batchSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubmissionOutbox>> GetBySubmissionIdAsync(Guid submissionId, CancellationToken cancellationToken);
}
