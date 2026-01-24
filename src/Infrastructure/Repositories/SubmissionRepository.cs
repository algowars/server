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
            .Select(
                (
                    outbox => new SubmissionOutboxModel
                    {
                        Id = outbox.Id,
                        Status = (SubmissionOutboxStatus)outbox.SubmissionOutboxStatusId,
                        Type = (SubmissionOutboxType)outbox.SubmissionOutboxTypeId,
                        SubmissionId = outbox.SubmissionId,
                        Submission = new SubmissionModel
                        {
                            Id = outbox.Submission!.Id,
                            ProblemSetupId = outbox.Submission.ProblemSetupId,
                            Code = outbox.Submission.Code,
                            CreatedOn = outbox.Submission.CreatedOn,
                            CreatedById = outbox.Submission.CreatedById,
                            Results = outbox.Submission.Results.Select(
                                result => new SubmissionResult
                                {
                                    Id = result.Id,
                                    Status = (SubmissionStatus)result.StatusId,
                                    FinishedAt = result.FinishedAt,
                                    MemoryKb = result.MemoryKb,
                                    RuntimeMs = result.RuntimeMs,
                                    StartedAt = result.StartedAt,
                                    Stdout = result.Stdout,
                                }
                            ),
                        },
                    }
                )
            )
            .ToListAsync(cancellationToken);
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
            .Select(
                (
                    outbox => new SubmissionOutboxModel
                    {
                        Id = outbox.Id,
                        Status = (SubmissionOutboxStatus)outbox.SubmissionOutboxStatusId,
                        Type = (SubmissionOutboxType)outbox.SubmissionOutboxTypeId,
                        SubmissionId = outbox.SubmissionId,
                        Submission = new SubmissionModel
                        {
                            Id = outbox.Submission!.Id,
                            ProblemSetupId = outbox.Submission.ProblemSetupId,
                            Code = outbox.Submission.Code,
                            CreatedOn = outbox.Submission.CreatedOn,
                            CreatedById = outbox.Submission.CreatedById,
                            Results = outbox.Submission.Results.Select(
                                result => new SubmissionResult
                                {
                                    Id = result.Id,
                                    Status = (SubmissionStatus)result.StatusId,
                                    FinishedAt = result.FinishedAt,
                                    MemoryKb = result.MemoryKb,
                                    RuntimeMs = result.RuntimeMs,
                                    StartedAt = result.StartedAt,
                                    Stdout = result.Stdout,
                                }
                            ),
                        },
                    }
                )
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<ProblemSubmissions?> GetProblemSubmissionsAsync(
        Guid problemId,
        CancellationToken cancellationToken
    )
    {
        return await db
            .Problems.Where(problem => problem.Id == problemId)
            .Select(problem => new ProblemSubmissions
            {
                Problem = new ProblemModel
                {
                    Id = problem.Id,
                    Title = problem.Title,
                    Slug = problem.Slug,
                    Question = problem.Question,
                    Difficulty = problem.Difficulty,
                    CreatedOn = problem.CreatedOn,
                    CreatedBy =
                        problem.CreatedBy != null
                            ? new AccountModel
                            {
                                Id = problem.CreatedBy.Id,
                                Username = problem.CreatedBy.Username,
                                ImageUrl = problem.CreatedBy.ImageUrl,
                            }
                            : null,
                    Tags = problem.Tags.Select(tag => new TagModel
                    {
                        Id = tag.Id,
                        Value = tag.Value,
                    }),
                },
                Submissions = problem
                    .ProblemSetups.SelectMany(setup => setup.Submissions)
                    .Select(submission => new SubmissionModel
                    {
                        Id = submission.Id,
                        Code = submission.Code,
                        CompletedAt = submission.CompletedAt,
                    }),
            })
            .SingleOrDefaultAsync(cancellationToken);
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

    private static readonly int MaxRetryCount = 5;
}
