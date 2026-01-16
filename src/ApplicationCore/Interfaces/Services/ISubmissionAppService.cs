using ApplicationCore.Dtos.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ISubmissionAppService
{
    Task<Result<Guid>> ExecuteAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    );

    Task<Result<SubmissionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
