using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
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
                    SubmissionOutboxTypeId = (int)SubmissionOutboxType.Initialized,
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

    public Task BulkUpsertAsync(
        IEnumerable<SubmissionModel> submissions,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
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
