using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Submission;
using Infrastructure.Persistence.Entities.Submission.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await db.Submissions.AddAsync(
                new SubmissionEntity
                {
                    Id = submission.Id,
                    ProblemSetupId = submission.ProblemSetupId,
                    Code = submission.Code ?? "",
                    CreatedOn = DateTime.UtcNow,
                    CreatedById = submission.CreatedById,
                },
                cancellationToken
            );

            await db.SubmissionOutbox.AddAsync(
                new SubmissionOutboxEntity
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    SubmissionOutboxTypeId = (int)SubmissionOutboxType.ExecuteSubmission,
                    SubmissionOutboxStatusId = (int)SubmissionOutboxStatus.Pending,
                },
                cancellationToken
            );

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public async Task BulkUpsertResultsAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    )
    {
        var resultEntities = submissions
            .SelectMany(
                s => s.Results,
                (s, sr) =>
                    new SubmissionResultEntity
                    {
                        Id = sr.Id,
                        SubmissionId = s.Id,
                        StatusId = (int)sr.Status,
                        StartedAt = sr.StartedAt,
                        FinishedAt = sr.FinishedAt,
                        Stdout = sr.Stdout,
                        RuntimeMs = sr.RuntimeMs,
                        MemoryKb = sr.MemoryKb,
                    }
            )
            .ToList();

        if (resultEntities.Count == 0)
        {
            return;
        }

        await db.BulkInsertOrUpdateAsync(resultEntities, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionExecutionOutboxesAsync(
        CancellationToken cancellationToken
    )
    {
        return await db
            .SubmissionOutbox.Where(outbox =>
                outbox.FinalizedOn == null && outbox.AttemptCount < MaxRetryCount
            )
            .Where(outbox =>
                outbox.SubmissionOutboxTypeId == (int)SubmissionOutboxType.ExecuteSubmission
            )
            .Include(outbox => outbox.Submission)
            .Select(MapOutboxExpr)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionPollingOutboxesAsync(
        CancellationToken cancellationToken
    )
    {
        return await db
            .SubmissionOutbox.Where(outbox =>
                outbox.FinalizedOn == null && outbox.AttemptCount < MaxRetryCount
            )
            .Where(outbox =>
                outbox.SubmissionOutboxTypeId == (int)SubmissionOutboxType.PollJudge0Result
            )
            .Include(outbox => outbox.Submission)
            .Select(MapOutboxExpr)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task MarkOutboxesAsPollingAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        await db
            .SubmissionOutbox.Where(o => outboxIds.Contains(o.Id))
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(o => o.AttemptCount, o => o.AttemptCount + 1)
                        .SetProperty(
                            o => o.SubmissionOutboxStatusId,
                            (int)SubmissionOutboxStatus.Processing
                        )
                        .SetProperty(
                            o => o.SubmissionOutboxTypeId,
                            (int)SubmissionOutboxType.PollJudge0Result
                        )
                        .SetProperty(o => o.ProcessOn, now)
                        .SetProperty(o => o.NextAttemptOn, (DateTime?)null)
                        .SetProperty(o => o.LastError, (string?)null),
                cancellationToken: cancellationToken
            );
    }

    public async Task ProcessSubmissionExecution(
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
                            Id = sr.Id,
                            SubmissionId = s.Id,
                            StatusId = (int)sr.Status,
                            StartedAt = sr.StartedAt,
                            FinishedAt = sr.FinishedAt,
                            Stdout = sr.Stdout,
                            RuntimeMs = sr.RuntimeMs,
                            MemoryKb = sr.MemoryKb,
                        }
                )
                .ToList();

            if (resultEntities.Count != 0)
            {
                await db.BulkInsertOrUpdateAsync(
                    resultEntities,
                    cancellationToken: cancellationToken
                );

                var submissionIds = resultEntities
                    .Select(re => re.SubmissionId)
                    .Distinct()
                    .ToList();

                await db
                    .SubmissionOutbox.Where(outbox =>
                        submissionIds.Contains(outbox.SubmissionId)
                        && outbox.SubmissionOutboxTypeId
                            == (int)SubmissionOutboxType.ExecuteSubmission
                    )
                    .ExecuteUpdateAsync(
                        setters =>
                            setters
                                .SetProperty(
                                    o => o.SubmissionOutboxTypeId,
                                    (int)SubmissionOutboxType.PollJudge0Result
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

    public async Task ProcessSubmissionPolling(
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
                            Id = sr.Id,
                            SubmissionId = s.Id,
                            StatusId = (int)sr.Status,
                            StartedAt = sr.StartedAt,
                            FinishedAt = sr.FinishedAt,
                            Stdout = sr.Stdout,
                            RuntimeMs = sr.RuntimeMs,
                            MemoryKb = sr.MemoryKb,
                        }
                )
                .ToList();

            if (resultEntities.Count != 0)
            {
                await db.BulkInsertOrUpdateAsync(
                    resultEntities,
                    cancellationToken: cancellationToken
                );

                var submissionIds = resultEntities
                    .Select(re => re.SubmissionId)
                    .Distinct()
                    .ToList();

                await db
                    .SubmissionOutbox.Where(outbox =>
                        submissionIds.Contains(outbox.SubmissionId)
                        && outbox.SubmissionOutboxTypeId
                            == (int)SubmissionOutboxType.PollJudge0Result
                    )
                    .ExecuteUpdateAsync(
                        setters =>
                            setters.SetProperty(
                                o => o.SubmissionOutboxTypeId,
                                (int)SubmissionOutboxType.PollJudge0Result
                            ),
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

    public Task IncrementOutboxesCount(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        return db
            .SubmissionOutbox.Where(o => outboxIds.Contains(o.Id))
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(o => o.AttemptCount, o => o.AttemptCount + 1)
                        .SetProperty(o => o.ProcessOn, now)
                        .SetProperty(o => o.NextAttemptOn, (DateTime?)null)
                        .SetProperty(o => o.LastError, (string?)null),
                cancellationToken: cancellationToken
            );
    }

    public async Task<PaginatedResult<SubmissionModel>> GetProblemSubmissions(
        Guid problemId,
        PaginationRequest pagination,
        CancellationToken cancellationToken
    )
    {
        int page = pagination.Page > 0 ? pagination.Page : 1;
        int size = pagination.Size > 0 ? pagination.Size : 10;

        var baseQuery = db
            .ProblemSetups.Where(ps => ps.ProblemId == problemId)
            .SelectMany(ps => ps.Submissions);

        int total = await baseQuery.CountAsync(cancellationToken: cancellationToken);

        var ordered =
            pagination.Direction == SortDirection.Asc
                ? baseQuery.OrderBy(s => s.CreatedOn).ThenBy(s => s.Id)
                : baseQuery.OrderByDescending(s => s.CreatedOn).ThenByDescending(s => s.Id);

        var submissions = await ordered
            .Skip((page - 1) * size)
            .Take(size)
            .Select(MapSubmissionExpr)
            .ToListAsync(cancellationToken: cancellationToken);

        return new PaginatedResult<SubmissionModel>
        {
            Results = submissions,
            Total = total,
            Page = page,
            Size = size,
        };
    }

    private static readonly int MaxRetryCount = 5;

    private static readonly Expression<
        Func<SubmissionResultEntity, SubmissionResult>
    > MapResultExpr = result => new SubmissionResult
    {
        Id = result.Id,
        Status = (SubmissionStatus)result.StatusId,
        FinishedAt = result.FinishedAt,
        MemoryKb = result.MemoryKb,
        RuntimeMs = result.RuntimeMs,
        StartedAt = result.StartedAt,
        Stdout = result.Stdout,
    };

    private static readonly Expression<Func<SubmissionEntity, SubmissionModel>> MapSubmissionExpr =
        submission => new SubmissionModel
        {
            Id = submission.Id,
            Code = submission.Code,
            ProblemSetupId = submission.ProblemSetupId,
            CreatedOn = submission.CreatedOn,
            CompletedAt = submission.CompletedAt,
            CreatedById = submission.CreatedById,
            Results = submission.Results.AsQueryable().Select(MapResultExpr),
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
}
