using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;

namespace ApplicationCore.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    );

    Task<Guid> SaveAsync(SubmissionModel submission, CancellationToken cancellationToken);

    Task IncrementOutboxesCountAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    );

    Task ProcessPollingSubmissionExecutionsAsync(
        IEnumerable<SubmissionModel> submissionModels, CancellationToken cancellationToken);

    Task ProcessSubmissionInitializationAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

}