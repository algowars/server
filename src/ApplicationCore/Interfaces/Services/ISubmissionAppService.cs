using ApplicationCore.Domain.Submissions.Outbox;
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

    Task<Result<IEnumerable<SubmissionOutboxModel>>> GetExecutionOutboxesAsync(
        CancellationToken cancellationToken
    );
}
