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
            var existingCode = await db
                .SubmissionCodes.Where(code => code.Code == submission.Code)
                .SingleOrDefaultAsync(cancellationToken);

            existingCode ??= new SubmissionCodeEntity
            {
                Id = Guid.NewGuid(),
                Code = submission.Code,
                CreatedOn = DateTime.UtcNow,
            };
            var entity = new SubmissionEntity
            {
                Id = submission.Id,
                ProblemSetupId = submission.ProblemSetupId,
                CodeId = existingCode.Id,
                Code = existingCode,
                CreatedOn = new DateTime(),
                CreatedById = submission.CreatedById,
            };

            var submissionOutbox = new SubmissionOutboxEntity
            {
                Id = Guid.NewGuid(),
                SubmissionId = entity.Id,
                TypeId = (int)SubmissionOutboxType.Initialized,
                AttemptCount = 0,
                CreatedOn = DateTime.UtcNow,
                StatusId = (int)SubmissionOutboxStatus.Pending,
            };

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SubmissionModel> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db
            .Submissions.Where(submission => submission.Id == id)
            .Select(submission => new SubmissionModel
            {
                Id = submission.Id,
                Code = submission.Code.Code,
                ProblemSetupId = submission.ProblemSetupId,
                CreatedOn = submission.CreatedOn,
                CreatedById = submission.CreatedById,
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
