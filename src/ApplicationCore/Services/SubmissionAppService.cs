using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Submissions.GetSubmissionExecutionOutboxes;
using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Services;

public sealed class SubmissionAppService(IMediator mediator) : ISubmissionAppService
{
    public Task<Result<Guid>> CreateAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateSubmissionCommand(problemSetupId, code, createdById);

        return mediator.Send(command, cancellationToken);
    }

    public Task<Result<IEnumerable<SubmissionOutboxModel>>> GetExecutionOutboxesAsync(
        CancellationToken cancellationToken
    )
    {
        var query = new GetSubmissionExecutionOutboxesCommand();

        return mediator.Send(query, cancellationToken);
    }
}
