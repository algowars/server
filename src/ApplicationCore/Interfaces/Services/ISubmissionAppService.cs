using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Dtos.Submissions;
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

    Task<Result<IEnumerable<SubmissionOutboxModel>>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    );

    Task<Result<Unit>> IncrementOutboxesCountAsync(
        IEnumerable<Guid> outboxIds,
        DateTime timestamp,
        CancellationToken cancellationToken
    );

    Task<Result<Unit>> SaveExecutionTokensAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    );

    Task<Result<Unit>> ProcessSubmissionExecutionAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    );

    Task<Result<Unit>> ProcessPollingSubmissionExecutionsAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    );

    Task<Result<Unit>> ProcessEvaluationAsync(
        IEnumerable<SubmissionModel> results,
        CancellationToken cancellationToken
    );

    Task<Result<Unit>> FinalizeEvaluationAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    );

    Task<Result<PaginatedResult<SubmissionDto>>> GetSubmissionsPaginatedAsync(
        Guid problemId,
        int page,
        int size,
        DateTime timestamp,
        Guid? filterByUserId = null,
        bool acceptedOnly = true,
        CancellationToken cancellationToken = default
    );
}