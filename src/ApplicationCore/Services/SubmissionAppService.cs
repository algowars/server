using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;
using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Submissions.GetSubmissionOutboxes;
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

    public Task<Result<IEnumerable<SubmissionOutboxModel>>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    )
    {
        var query = new GetSubmissionOutboxesQuery();

        return mediator.Send(query, cancellationToken);
    }

    public Task<Result<Unit>> IncrementOutboxesCountAsync(
        IEnumerable<Guid> outboxIds,
        DateTime timestamp,
        CancellationToken cancellationToken
    )
    {
        var command = new IncrementSubmissionOutboxesCommand(outboxIds, timestamp);

        return mediator.Send(command, cancellationToken);
    }

    public Task<Result<Unit>> ProcessSubmissionExecutionAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessSubmissionExecutionsCommand(results);

        return mediator.Send(command, cancellationToken);
    }
}
