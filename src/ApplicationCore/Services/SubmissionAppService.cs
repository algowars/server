using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;
using ApplicationCore.Commands.Submissions.ProcessPollExecution;
using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Commands.Submissions.ProcessSubmissionPolling;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Submissions.GetSubmissionOutboxes;
using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Services;

public sealed class SubmissionAppService(IMediator mediator) : ISubmissionAppService
{
    public async Task<Result<Guid>> CreateAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateSubmissionCommand(problemSetupId, code, createdById);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<IEnumerable<SubmissionOutboxModel>>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    )
    {
        var query = new GetSubmissionOutboxesQuery();

        return await mediator.Send(query, cancellationToken);
    }

    public async Task<Result<Unit>> IncrementOutboxesCountAsync(
        IEnumerable<Guid> outboxIds,
        DateTime timestamp,
        CancellationToken cancellationToken
    )
    {
        var command = new IncrementSubmissionOutboxesCommand(outboxIds, timestamp);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<Unit>> ProcessSubmissionExecutionAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessSubmissionExecutionsCommand(results);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<Unit>> ProcessPollExecutionAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessPollExecutionsCommand(results);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<Unit>> ProcessSubmissionPollingAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessSubmissionPollingCommand(results);

        return await mediator.Send(command, cancellationToken);
    }
}
