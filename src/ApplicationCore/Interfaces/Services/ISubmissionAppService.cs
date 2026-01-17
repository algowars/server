using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ISubmissionAppService
{
    Task<Result<Guid>> CreateAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    );
}
