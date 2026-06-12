using Algowars.Domain.Submissions.Entities;

namespace Algowars.Domain.Submissions;

public interface ISubmissionRepository
{
    Task AddAsync(Submission submission, CancellationToken cancellationToken = default);
    Task<Submission?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Submission submission, CancellationToken cancellationToken = default);
}
