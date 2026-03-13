using System.Linq.Expressions;
using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Submission;
using Infrastructure.Persistence.Entities.Submission.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    )
    {
        return await db
            .SubmissionOutboxes.Where(outbox =>
                outbox.FinalizedOn == null && outbox.AttemptCount < MaxRetryCount
            )
            .Include(outbox => outbox.Submission)
            .Select(MapOutboxExpr)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken)
    {
        DateTime createdOn = DateTime.UtcNow;
        db.Submissions.Add(
            new SubmissionEntity
            {
                Id = submission.Id,
                ProblemSetupId = submission.ProblemSetupId,
                Code = submission.Code ?? "",
                CreatedOn = createdOn,
                CreatedById = submission.CreatedById,
            }
        );

        db.SubmissionOutboxes.Add(
            new SubmissionOutboxEntity
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                SubmissionOutboxTypeId = (int)SubmissionOutboxType.Initialized,
                SubmissionOutboxStatusId = (int)SubmissionOutboxStatus.Pending,
                CreatedOn = createdOn,
            }
        );

        await db.SaveChangesAsync(cancellationToken);
    }

    public Task IncrementOutboxesCountAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        return db
            .SubmissionOutboxes.Where(o => outboxIds.Contains(o.Id))
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(o => o.AttemptCount, o => o.AttemptCount + 1)
                        .SetProperty(o => o.ProcessOn, now)
                        .SetProperty(o => o.NextAttemptOn, (DateTime?)null),
                cancellationToken: cancellationToken
            );
    }

    public async Task ProcessSubmissionInitializationAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var resultEntities = submissions
                .SelectMany(
                    s => s.Results,
                    (s, sr) =>
                        new SubmissionResultEntity
                        {
                            Id = Guid.NewGuid(),
                            ExecutionId = sr.Id,
                            SubmissionId = s.Id,
                            StatusId = (int)sr.Status,
                            StartedAt = sr.StartedAt,
                            FinishedAt = sr.FinishedAt,
                            RuntimeMs = sr.RuntimeMs,
                            MemoryKb = sr.MemoryKb,
                            ExpectedOutput = sr.ExpectedOutput,
                        }
                )
                .ToList();

            await db.BulkInsertOrUpdateAsync(resultEntities, cancellationToken: cancellationToken);

            var submissionIds = resultEntities.Select(re => re.SubmissionId).Distinct().ToList();

            await db
                .SubmissionOutboxes.Where(outbox =>
                    submissionIds.Contains(outbox.SubmissionId)
                    && outbox.SubmissionOutboxTypeId == (int)SubmissionOutboxType.Initialized
                )
                .ExecuteUpdateAsync(
                    setters =>
                        setters
                            .SetProperty(
                                o => o.SubmissionOutboxTypeId,
                                (int)SubmissionOutboxType.PollEvaluation
                            )
                            .SetProperty(o => o.AttemptCount, _ => 0),
                    cancellationToken: cancellationToken
                );

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ProcessPollExecutionAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var submissionList = submissions.ToList();

            var executionIds = submissionList
                .SelectMany(s => s.Results.Select(r => r.Id))
                .ToList();

            var existingEntities = await db
                .SubmissionResults.Where(r => executionIds.Contains(r.ExecutionId))
                .ToListAsync(cancellationToken);

            var entityMap = existingEntities.ToDictionary(e => e.ExecutionId);

            var resultUpdates = submissionList.SelectMany(
                s => s.Results,
                (s, r) => (SubmissionId: s.Id, Result: r)
            );

            foreach (var (_, result) in resultUpdates)
            {
                if (!entityMap.TryGetValue(result.Id, out var entity))
                {
                    continue;
                }

                entity.StatusId = (int)result.Status;
                entity.Stdout = result.Stdout;
                entity.RuntimeMs = result.RuntimeMs;
                entity.MemoryKb = result.MemoryKb;
                entity.FinishedAt = result.FinishedAt;
            }

            await db.BulkUpdateAsync(existingEntities, cancellationToken: cancellationToken);

            var completedSubmissionIds = submissionList
                .Where(s => s.GetOverallStatus() != SubmissionStatus.Processing)
                .Select(s => s.Id)
                .ToList();

            if (completedSubmissionIds.Count > 0)
            {
                var now = DateTime.UtcNow;

                await db
                    .SubmissionOutboxes.Where(o =>
                        completedSubmissionIds.Contains(o.SubmissionId)
                        && o.SubmissionOutboxTypeId == (int)SubmissionOutboxType.PollEvaluation
                    )
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(o => o.FinalizedOn, now),
                        cancellationToken: cancellationToken
                    );

                await db
                    .Submissions.Where(s => completedSubmissionIds.Contains(s.Id))
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(s => s.CompletedAt, now),
                        cancellationToken: cancellationToken
                    );
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ProcessSubmissionPollingAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var submissionList = submissions.ToList();

            var executionIds = submissionList
                .SelectMany(s => s.Results.Select(r => r.Id))
                .ToList();

            var existingEntities = await db
                .SubmissionResults.Where(r => executionIds.Contains(r.ExecutionId))
                .ToListAsync(cancellationToken);

            var entityMap = existingEntities.ToDictionary(e => e.ExecutionId);

            foreach (var (_, result) in submissionList.SelectMany(s => s.Results, (s, r) => (s.Id, r)))
            {
                if (!entityMap.TryGetValue(result.Id, out var entity))
                {
                    continue;
                }

                entity.StatusId = (int)result.Status;
                entity.Stdout = result.Stdout;
                entity.RuntimeMs = result.RuntimeMs;
                entity.MemoryKb = result.MemoryKb;
                entity.FinishedAt = result.FinishedAt;
            }

            await db.BulkUpdateAsync(existingEntities, cancellationToken: cancellationToken);

            var completedSubmissionIds = submissionList
                .Where(s => s.GetOverallStatus() != SubmissionStatus.Processing)
                .Select(s => s.Id)
                .ToList();

            if (completedSubmissionIds.Count > 0)
            {
                await db
                    .SubmissionOutboxes.Where(o =>
                        completedSubmissionIds.Contains(o.SubmissionId)
                        && o.SubmissionOutboxTypeId == (int)SubmissionOutboxType.PollExecution
                    )
                    .ExecuteUpdateAsync(
                        setters =>
                            setters
                                .SetProperty(
                                    o => o.SubmissionOutboxTypeId,
                                    (int)SubmissionOutboxType.EvaluateSubmission
                                )
                                .SetProperty(o => o.AttemptCount, _ => 0),
                        cancellationToken: cancellationToken
                    );
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ProcessEvaluationAsync(
        IEnumerable<ComparisonContext> contexts,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var contextList = contexts.ToList();

            var allBuiltResults = contextList.SelectMany(c => c.BuiltResults).ToList();
            var executionIds = allBuiltResults.Select(r => r.ExecutionId).ToList();

            var existingEntities = await db
                .SubmissionResults.Where(r => executionIds.Contains(r.ExecutionId))
                .ToListAsync(cancellationToken);

            var entityMap = existingEntities.ToDictionary(e => e.ExecutionId);

            foreach (var builtResult in allBuiltResults)
            {
                if (!entityMap.TryGetValue(builtResult.ExecutionId, out var entity))
                {
                    continue;
                }

                entity.ResultId = builtResult.ResultId;
            }

            await db.BulkUpdateAsync(existingEntities, cancellationToken: cancellationToken);

            var submissionIds = contextList.Select(c => c.SubmissionId).Distinct().ToList();

            await db
                .SubmissionOutboxes.Where(o =>
                    submissionIds.Contains(o.SubmissionId)
                    && o.SubmissionOutboxTypeId == (int)SubmissionOutboxType.EvaluateSubmission
                )
                .ExecuteUpdateAsync(
                    setters =>
                        setters
                            .SetProperty(
                                o => o.SubmissionOutboxTypeId,
                                (int)SubmissionOutboxType.PollEvaluation
                            )
                            .SetProperty(o => o.AttemptCount, _ => 0),
                    cancellationToken: cancellationToken
                );

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static readonly Expression<
        Func<SubmissionResultEntity, SubmissionResult>
    > MapResultExpr = result => new SubmissionResult
    {
        Id = result.ExecutionId,
        Status = (SubmissionStatus)result.StatusId,
        FinishedAt = result.FinishedAt,
        MemoryKb = result.MemoryKb,
        RuntimeMs = result.RuntimeMs,
        StartedAt = result.StartedAt,
        Stdout = result.Stdout,
        ExpectedOutput = result.ExpectedOutput,
    };

    private static readonly Expression<
        Func<SubmissionOutboxEntity, SubmissionOutboxModel>
    > MapOutboxExpr = outbox => new SubmissionOutboxModel
    {
        Id = outbox.Id,
        Status = (SubmissionOutboxStatus)outbox.SubmissionOutboxStatusId,
        Type = (SubmissionOutboxType)outbox.SubmissionOutboxTypeId,
        SubmissionId = outbox.SubmissionId,
        Submission =
            outbox.Submission == null
                ? null!
                : new SubmissionModel
                {
                    Id = outbox.Submission.Id,
                    ProblemSetupId = outbox.Submission.ProblemSetupId,
                    Code = outbox.Submission.Code,
                    CreatedOn = outbox.Submission.CreatedOn,
                    CreatedById = outbox.Submission.CreatedById,
                    Results = outbox.Submission.Results.AsQueryable().Select(MapResultExpr),
                },
    };

    private const int MaxRetryCount = 5;
}
