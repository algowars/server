using Algowars.Domain.Submissions.Entities;

namespace Algowars.Domain.Submissions;

public interface ISubmissionRepository
{
    Task AddAsync(Submission submission, CancellationToken cancellationToken);
    Task<Submission?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateAsync(Submission submission, CancellationToken cancellationToken);
}
