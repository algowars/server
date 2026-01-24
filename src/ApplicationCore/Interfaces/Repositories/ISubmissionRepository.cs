using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;

namespace ApplicationCore.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken);

    Task ProcessSubmissionExecution(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    Task ProcessSubmissionPolling(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    Task IncrementOutboxesCount(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    );

    Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionExecutionOutboxesAsync(
        CancellationToken cancellationToken
    );

    Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionPollingOutboxesAsync(
        CancellationToken cancellationToken
    );
}
