using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Submission;
using Infrastructure.Persistence.Entities.Submission.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
                .ThenInclude(submission => submission.Results)
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

    public async Task ProcessPollingSubmissionExecutionsAsync(
        IEnumerable<SubmissionModel> submissionModels,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var resultEntities = submissionModels
                .SelectMany(
                    s => s.Results,
                    (s, sr) =>
                        new SubmissionResultEntity
                        {
                            Id = sr.Id,
                            SubmissionId = s.Id,
                            ExecutionId = sr.ExecutionId,
                            StatusId = sr.Status
                                is SubmissionStatus.Accepted
                                    or SubmissionStatus.WrongAnswer
                                ? (int)SubmissionStatus.Processing
                                : (int)sr.Status,
                            StartedAt = sr.StartedAt,
                            FinishedAt = sr.FinishedAt,
                            ProgramOutput = sr.Stdout,
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

                var completedSubmissionIds = submissionModels
                    .Where(s =>
                        s.Results.Any()
                        && s.Results.All(r =>
                            r.Status
                                is not SubmissionStatus.InQueue
                                    and not SubmissionStatus.Processing
                        )
                    )
                    .Select(s => s.Id)
                    .Distinct()
                    .ToList();

                if (completedSubmissionIds.Count != 0)
                {
                    await db
                        .SubmissionOutboxes.Where(outbox =>
                            completedSubmissionIds.Contains(outbox.SubmissionId)
                            && outbox.SubmissionOutboxTypeId
                                == (int)SubmissionOutboxType.PollExecution
                        )
                        .ExecuteUpdateAsync(
                            setters =>
                                setters
                                    .SetProperty(
                                        o => o.SubmissionOutboxTypeId,
                                        (int)SubmissionOutboxType.Evaluate
                                    )
                                    .SetProperty(o => o.AttemptCount, _ => 0),
                            cancellationToken: cancellationToken
                        );
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
                            SubmissionId = s.Id,
                            StatusId = (int)sr.Status,
                            ExecutionId = sr.ExecutionId,
                            StartedAt = sr.StartedAt,
                            FinishedAt = sr.FinishedAt,
                            ProgramOutput = sr.Stdout,
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
                    .SubmissionOutboxes.Where(outbox =>
                        submissionIds.Contains(outbox.SubmissionId)
                        && outbox.SubmissionOutboxTypeId == (int)SubmissionOutboxType.Initialized
                    )
                    .ExecuteUpdateAsync(
                        setters =>
                            setters
                                .SetProperty(
                                    o => o.SubmissionOutboxTypeId,
                                    (int)SubmissionOutboxType.PollExecution
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

    private static readonly Expression<
        Func<SubmissionResultEntity, SubmissionResult>
    > MapResultExpr = result => new SubmissionResult
    {
        Id = result.Id,
        Status = (SubmissionStatus)result.StatusId,
        ExecutionId = result.ExecutionId,
        ResultId = result.ResultId,
        FinishedAt = result.FinishedAt,
        MemoryKb = result.MemoryKb,
        RuntimeMs = result.RuntimeMs,
        StartedAt = result.StartedAt,
        Stdout = result.Stdout,
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