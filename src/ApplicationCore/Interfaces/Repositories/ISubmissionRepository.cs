using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;

namespace ApplicationCore.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken);

    Task BulkUpsertResultsAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    );
}
