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
                    Code = submission.Code,
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
        catch (Exception ex)
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

    public async Task<IEnumerable<SubmissionOutboxModel>> GetSubmissionOutboxesAsync(
        CancellationToken cancellationToken
    )
    {
        return await db
            .SubmissionOutbox.Where(outbox => outbox.FinalizedOn == null)
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
}
