using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;

namespace ApplicationCore.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    );

    Task<Guid> SaveAsync(SubmissionModel submission, CancellationToken cancellationToken);

    Task IncrementOutboxesCountAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    );

    Task SaveExecutionTokensAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    Task ProcessPollingSubmissionExecutionsAsync(
        IEnumerable<SubmissionModel> submissionModels,
        CancellationToken cancellationToken
    );

    Task ProcessEvaluationAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    Task FinalizeEvaluationAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    );

    Task ProcessSubmissionInitializationAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    Task<PaginatedResult<SubmissionModel>> GetSolutionsByProblemId(Guid problemId, PaginationRequest pagination, CancellationToken cancellationToken);

    Task<PaginatedResult<SubmissionModel>> GetUserSolutionsByProblemId(Guid problemId, Guid accountId, PaginationRequest pagination, SubmissionStatus? statusFilter, CancellationToken cancellationToken);
}