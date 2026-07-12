using Algowars.Domain.SeedWork;

namespace Algowars.Domain.SubmissionJobs;

public interface ISubmissionJobRepository : IRepository<SubmissionJob>
{
    Task<IReadOnlyList<SubmissionJob>> FindPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<SubmissionJob?> FindBySubmissionIdAsync(Guid submissionId, CancellationToken cancellationToken = default);
}
