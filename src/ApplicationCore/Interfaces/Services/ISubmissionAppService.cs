using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;
using Ardalis.Result;
using MediatR;

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
