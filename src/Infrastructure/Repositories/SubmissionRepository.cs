using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;

namespace Infrastructure.Repositories;

internal class SubmissionRepository : ISubmissionRepository
{
    public Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
