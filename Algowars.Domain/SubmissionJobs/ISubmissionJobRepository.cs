using Algowars.Domain.SeedWork;
using Algowars.Domain.SubmissionJobs.Entities;

namespace Algowars.Domain.SubmissionJobs;

public interface ISubmissionJobRepository : IRepository<SubmissionJob>
{
    Task<IReadOnlyList<SubmissionJob>> FindPendingAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<SubmissionJob?> FindBySubmissionIdAsync(Guid submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Immediately inserts a newly created attempt into the database.
    /// Call this right after <see cref="SubmissionJob.StartAttempt"/> so the row
    /// exists before the step handler runs, making the final <see cref="UpdateAsync"/>
    /// a pure UPDATE with no INSERT required.
    /// </summary>
    Task PersistAttemptAsync(SubmissionJob job, SubmissionJobAttempt attempt, CancellationToken cancellationToken = default);
}
