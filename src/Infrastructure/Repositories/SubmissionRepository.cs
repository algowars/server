using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Submission;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var codeEntity = await db
            .SubmissionCodes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == submission.Code, cancellationToken);

        if (codeEntity is null)
        {
            codeEntity = new SubmissionCodeEntity
            {
                Id = Guid.NewGuid(),
                Code = submission.Code,
                CreatedOn = submission.CreatedOn,
            };

            db.SubmissionCodes.Add(codeEntity);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                codeEntity = await db
                    .SubmissionCodes.AsNoTracking()
                    .SingleAsync(c => c.Code == submission.Code, cancellationToken);
            }
        }

        var submissionEntity = new SubmissionEntity
        {
            Id = submission.Id,
            CodeId = codeEntity.Id,
            ProblemSetupId = submission.ProblemSetupId,
            CreatedOn = submission.CreatedOn,
            CompletedAt = submission.CompletedAt,
            CreatedById = submission.CreatedById,
        };

        db.Submissions.Add(submissionEntity);
        await db.SaveChangesAsync(cancellationToken);

        if (submission.Results.Any())
        {
            var resultEntities = submission.Results.Select(r => new SubmissionResultEntity
            {
                Id = r.Id,
                SubmissionId = submissionEntity.Id,
                StatusId = (int)r.Status,
                Stdout = r.Stdout,
                Stderr = r.Stderr,
                RuntimeMs = r.RuntimeMs is null ? null : (int?)Math.Ceiling(r.RuntimeMs.Value),
                MemoryKb = r.MemoryKb,
            });

            db.SubmissionResults.AddRange(resultEntities);
            await db.SaveChangesAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }
}
