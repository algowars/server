using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities.Submission;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task SaveAsync(SubmissionModel submission, CancellationToken cancellationToken)
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

        await db.SaveChangesAsync(cancellationToken);
    }
}
