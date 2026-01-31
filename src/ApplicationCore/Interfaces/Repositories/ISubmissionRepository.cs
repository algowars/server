using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;

namespace ApplicationCore.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    );

    Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken);

    Task IncrementOutboxesCount(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    );

    Task ProcessSubmissionInitialization(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );
}
