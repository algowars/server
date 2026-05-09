using System.Linq.Expressions;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Submission;
using Infrastructure.Persistence.Entities.Submission.Outbox;
using Mapster;
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
                .ThenInclude(submission => submission.Results)
            .Select(MapOutboxExpr)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Guid> SaveAsync(
        SubmissionModel submission,
        CancellationToken cancellationToken
    )
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

        var outboxId = Guid.NewGuid();
        db.SubmissionOutboxes.Add(
            new SubmissionOutboxEntity
            {
                Id = outboxId,
                SubmissionId = submission.Id,
                SubmissionOutboxTypeId = (int)SubmissionOutboxType.Initialized,
                SubmissionOutboxStatusId = (int)SubmissionOutboxStatus.Pending,
                CreatedOn = createdOn,
            }
        );

        await db.SaveChangesAsync(cancellationToken);
        return outboxId;
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
                            ResultId = sr.ResultId ?? sr.Id,
                            StatusId = sr.Status
                                is SubmissionStatus.Accepted
                                    or SubmissionStatus.WrongAnswer
                                ? (int)SubmissionStatus.Processing
                                : (int)sr.Status,
                            StartedAt = sr.StartedAt,
                            FinishedAt = sr.FinishedAt,
                            Stdout = sr.Stdout,
                            ProgramOutput = sr.ProgramOutput,
                            Stderr = sr.Stderr,
                            RuntimeMs = sr.RuntimeMs,
                            MemoryKb = sr.MemoryKb,
                        }
                )
                .ToList();

            if (resultEntities.Count != 0)
            {
                await db.BulkInsertOrUpdateAsync(
                    resultEntities,
                    ResultBulkConfig,
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
                            Id = sr.Id,
                            SubmissionId = s.Id,
                            ExecutionId = sr.ExecutionId,
                            ResultId = sr.Id,
                            StatusId = (int)SubmissionStatus.InQueue,
                            CreatedOn = DateTime.UtcNow,
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
                    ResultBulkConfig,
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
        ProgramOutput = result.ProgramOutput,
        Stderr = result.Stderr,
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

    private static readonly Expression<Func<SubmissionEntity, SubmissionModel>> MapSubmissionExpr =
        submission => new SubmissionModel
        {
            Id = submission.Id,
            Code = submission.Code,
            ProblemSetupId = submission.ProblemSetupId,
            CreatedOn = submission.CreatedOn,
            CompletedAt = submission.CompletedAt,
            CreatedById = submission.CreatedById,
            LanguageVersion = new LanguageVersion
            {
                Id = submission.ProblemSetup!.LanguageVersion!.Id,
                Version = submission.ProblemSetup.LanguageVersion.Version,
                InitialCode = submission.ProblemSetup.LanguageVersion.InitialCode,
                ProgrammingLanguageId = submission
                    .ProblemSetup
                    .LanguageVersion
                    .ProgrammingLanguageId,
                Judge0LanguageId = null,
                ProgrammingLanguage =
                    submission.ProblemSetup.LanguageVersion.ProgrammingLanguage == null
                        ? null
                        : new ProgrammingLanguage
                        {
                            Id = submission.ProblemSetup.LanguageVersion.ProgrammingLanguage.Id,
                            Name = submission.ProblemSetup.LanguageVersion.ProgrammingLanguage.Name,
                            IsArchived = submission
                                .ProblemSetup
                                .LanguageVersion
                                .ProgrammingLanguage
                                .IsArchived,
                        },
            },
            CreatedBy =
                submission.CreatedBy == null
                    ? null
                    : new AccountModel
                    {
                        Username = submission.CreatedBy.Username,
                        ImageUrl = submission.CreatedBy.ImageUrl,
                        CreatedOn = submission.CreatedBy.CreatedOn,
                        Id = submission.CreatedBy.Id,
                    },
            Results = submission
                .Results.Select(result => new SubmissionResult
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
                    ProgramOutput = result.ProgramOutput,
                    Stderr = result.Stderr,
                })
                .ToList(),
        };

    private const int MaxRetryCount = 5;

    private static readonly BulkConfig ResultBulkConfig = new()
    {
        PropertiesToExcludeOnUpdate = [nameof(SubmissionResultEntity.CreatedOn)],
    };

    public async Task SaveExecutionTokensAsync(
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
                            ExecutionId = sr.ExecutionId,
                            ResultId = sr.ResultId,
                            StatusId = (int)SubmissionStatus.InQueue,
                            CreatedOn = DateTime.UtcNow,
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
                    ResultBulkConfig,
                    cancellationToken: cancellationToken
                );

                var submissionIds = resultEntities.Select(r => r.SubmissionId).Distinct().ToList();

                await db
                    .SubmissionOutboxes.Where(o =>
                        submissionIds.Contains(o.SubmissionId)
                        && o.SubmissionOutboxTypeId == (int)SubmissionOutboxType.Initialized
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

    public async Task ProcessEvaluationAsync(
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
                            ExecutionId = sr.ExecutionId,
                            ResultId = sr.ResultId,
                            StatusId = (int)sr.Status,
                            StartedAt = sr.StartedAt,
                            FinishedAt = sr.FinishedAt,
                            Stdout = sr.Stdout,
                            ProgramOutput = sr.ProgramOutput,
                            Stderr = sr.Stderr,
                            RuntimeMs = sr.RuntimeMs,
                            MemoryKb = sr.MemoryKb,
                        }
                )
                .ToList();

            if (resultEntities.Count != 0)
            {
                await db.BulkInsertOrUpdateAsync(
                    resultEntities,
                    ResultBulkConfig,
                    cancellationToken: cancellationToken
                );

                var submissionIds = resultEntities.Select(r => r.SubmissionId).Distinct().ToList();

                await db
                    .SubmissionOutboxes.Where(o =>
                        submissionIds.Contains(o.SubmissionId)
                        && o.SubmissionOutboxTypeId == (int)SubmissionOutboxType.Evaluate
                    )
                    .ExecuteUpdateAsync(
                        setters =>
                            setters
                                .SetProperty(
                                    o => o.SubmissionOutboxTypeId,
                                    (int)SubmissionOutboxType.EvaluationPoll
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

    public async Task FinalizeEvaluationAsync(
        IEnumerable<Guid> outboxIds,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        var ids = outboxIds.ToList();

        var submissionIds = await db
            .SubmissionOutboxes.Where(o => ids.Contains(o.Id))
            .Select(o => o.SubmissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        await db
            .Submissions.Where(s => submissionIds.Contains(s.Id))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.CompletedAt, now),
                cancellationToken: cancellationToken
            );

        await db
            .SubmissionOutboxes.Where(o => ids.Contains(o.Id))
            .ExecuteUpdateAsync(
                setters =>
                    setters.SetProperty(o => o.FinalizedOn, now).SetProperty(o => o.ProcessOn, now),
                cancellationToken: cancellationToken
            );
    }

    public async Task<PaginatedResult<SubmissionModel>> GetSubmissionsByProblemId(
        Guid problemId,
        Guid? accountId,
        PaginationRequest pagination,
        SubmissionStatus? statusFilter,
        CancellationToken cancellationToken
    )
    {
        IQueryable<SubmissionEntity> query = db
            .Submissions.Where(s =>
                s.ProblemSetup!.ProblemId == problemId
                && (accountId == null || s.CreatedById == accountId)
                && s.CreatedOn <= pagination.Timestamp
            )
            .Include(s => s.CreatedBy)
            .Include(s => s.Results)
            .Include(s => s.ProblemSetup)
                .ThenInclude(ps => ps!.LanguageVersion)
                    .ThenInclude(lv => lv!.ProgrammingLanguage);

        if (statusFilter.HasValue)
        {
            query = query.Where(s => s.Results.All(r => r.StatusId == (int)statusFilter.Value));
        }

        int total = await query.CountAsync(cancellationToken);

        var submissions = await query
            .OrderByDescending(s => s.CreatedOn)
            .Skip((pagination.Page - 1) * pagination.Size)
            .Take(pagination.Size)
            .Select(MapSubmissionExpr)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<SubmissionModel>
        {
            Results = submissions,
            Total = total,
            Page = pagination.Page,
            Size = pagination.Size,
        };
    }

    public async Task<SubmissionModel?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        var entity = await db.Submissions
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (entity == null) return null;

        return new SubmissionModel
        {
            Id = entity.Id,
            Code = entity.Code,
            ProblemSetupId = entity.ProblemSetupId,
            CreatedOn = entity.CreatedOn,
            CompletedAt = entity.CompletedAt,
            CreatedById = entity.CreatedById,
            Results = entity.Results.Select(r => new SubmissionResult
            {
                Id = r.Id,
                Status = (SubmissionStatus)r.StatusId,
                ExecutionId = r.ExecutionId,
                ResultId = r.ResultId,
                FinishedAt = r.FinishedAt,
                MemoryKb = r.MemoryKb,
                RuntimeMs = r.RuntimeMs,
                StartedAt = r.StartedAt,
                Stdout = r.Stdout,
                ProgramOutput = r.ProgramOutput,
                Stderr = r.Stderr,
            }).ToList(),
        };
    }
}
