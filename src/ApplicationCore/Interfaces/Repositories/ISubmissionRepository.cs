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

    /// <summary>
    /// Persists Judge0 execution tokens on submission results and transitions
    /// outbox type from <see cref="SubmissionOutboxType.Initialized"/> to
    /// <see cref="SubmissionOutboxType.PollExecution"/>.
    /// </summary>
    Task SaveExecutionTokensAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Persists polled stdout/status from Judge0 and transitions the outbox
    /// from <see cref="SubmissionOutboxType.PollExecution"/> to
    /// <see cref="SubmissionOutboxType.Evaluate"/> once all results are finished.
    /// </summary>
    Task ProcessPollingSubmissionExecutionsAsync(
        IEnumerable<SubmissionModel> submissionModels,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Persists comparison results (Accepted / WrongAnswer) and transitions
    /// outbox from <see cref="SubmissionOutboxType.Evaluate"/> to
    /// <see cref="SubmissionOutboxType.EvaluationPoll"/>.
    /// </summary>
    Task ProcessEvaluationAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Sets <c>FinalizedOn</c> on the outbox, completing the pipeline.
    /// </summary>
    Task FinalizeEvaluationAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    );

    Task ProcessSubmissionInitializationAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    );
}
