using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;

namespace ApplicationCore.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken);

    Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    );
}
