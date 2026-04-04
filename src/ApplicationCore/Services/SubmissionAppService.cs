using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;
using ApplicationCore.Commands.Submissions.ProcessPollingSubmissionExecutions;
using ApplicationCore.Commands.Submissions.ProcessPollingSubmissionReportExecutions;
using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Commands.Submissions.ProcessSubmissionReportExecution;
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

    public async Task<Result<Unit>> ProcessPollingSubmissionExecutionsAsync(IEnumerable<SubmissionModel> results, CancellationToken cancellationToken)
    {
        var command = new ProcessPollingSubmissionExecutionsCommand(results);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<Unit>> ProcessSubmissionReportExecutionAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessSubmissionReportExecutionCommand(results);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<Unit>> ProcessPollingSubmissionReportExecutionsAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessPollingSubmissionReportExecutionsCommand(results);

        return await mediator.Send(command, cancellationToken);
    }
}