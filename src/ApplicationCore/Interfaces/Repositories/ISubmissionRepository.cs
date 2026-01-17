using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken);
}
