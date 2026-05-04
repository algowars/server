using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Commands.Submissions.FinalizeEvaluation;
using ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;
using ApplicationCore.Commands.Submissions.ProcessEvaluation;
using ApplicationCore.Commands.Submissions.ProcessPollingSubmissionExecutions;
using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Commands.Submissions.SaveExecutionTokens;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Dtos.Submissions;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Submissions.GetSolutionsByProblemIdQuery;
using ApplicationCore.Queries.Submissions.GetSubmissionOutboxes;
using ApplicationCore.Queries.Submissions.GetSubmissionsPaginated;
using ApplicationCore.Queries.Submissions.GetUserSubmissionsByProblemIdQuery;
using Ardalis.Result;
using MediatR;
using static ApplicationCore.Logging.LoggingEventIds;

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

    public async Task<Result<Unit>> SaveExecutionTokensAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new SaveExecutionTokensCommand(results);

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

    public async Task<Result<Unit>> ProcessPollingSubmissionExecutionsAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessPollingSubmissionExecutionsCommand(results);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<Unit>> ProcessEvaluationAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    )
    {
        var command = new ProcessEvaluationCommand(results);

        return await mediator.Send(command, cancellationToken);
    }

    public async Task<Result<Unit>> FinalizeEvaluationAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        var command = new FinalizeEvaluationCommand(outboxIds, now);

        return await mediator.Send(command, cancellationToken);
    }

    public Task<Result<PaginatedResult<ProblemSubmissionDto>>> GetSolutionsAsync(Guid problemId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
    {
        var query = new GetSolutionsByProblemIdQuery(problemId, paginationRequest);
        return mediator.Send(query, cancellationToken);
    }

    public Task<Result<PaginatedResult<ProblemSubmissionDto>>> GetSubmissionsPaginatedAsync(Guid problemId, Guid accountId, PaginationRequest paginationRequest, CancellationToken cancellationToken = default)
    {
        var query = new GetUserSubmissionsByProblemIdQuery(problemId, accountId, paginationRequest);
        return mediator.Send(query, cancellationToken);
    }
}