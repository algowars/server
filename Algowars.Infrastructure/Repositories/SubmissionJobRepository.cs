using Algowars.Domain.SubmissionJobs;
using Algowars.Domain.SubmissionJobs.Entities;
using Algowars.Domain.SubmissionJobs.Enums;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class SubmissionJobRepository(AlgowarsDbContext context)
    : ISubmissionJobRepository
{
    public async Task AddAsync(SubmissionJob entity, CancellationToken cancellationToken = default)
    {
        await context.SubmissionJobs.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SubmissionJob?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.SubmissionJobs
            .Include(j => j.Attempts)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SubmissionJob>> FindPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
        => await context.SubmissionJobs
            .Include(j => j.Attempts)
            .Where(j => j.Status == SubmissionJobStatus.Pending || j.Status == SubmissionJobStatus.Running)
            .OrderBy(j => j.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public async Task<SubmissionJob?> FindBySubmissionIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default)
        => await context.SubmissionJobs
            .Include(j => j.Attempts)
            .FirstOrDefaultAsync(j => j.SubmissionId == submissionId, cancellationToken);

    public async Task UpdateAsync(SubmissionJob entity, CancellationToken cancellationToken = default)
    {
        // The attempt was already INSERT-ed by PersistAttemptAsync before the handler ran.
        // All entities are tracked; SaveChangesAsync detects property changes via snapshot
        // and issues only UPDATEs — no INSERT needed here.
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task PersistAttemptAsync(SubmissionJob job, SubmissionJobAttempt attempt, CancellationToken cancellationToken = default)
    {
        // Explicitly track the new attempt as Added and wire up the shadow FK,
        // then flush it to the DB immediately. This guarantees the row exists
        // before the step handler runs so that UpdateAsync only ever does UPDATEs.
        var entry = context.Entry(attempt);
        entry.State = EntityState.Added;
        entry.Property("job_id").CurrentValue = job.Id;
        await context.SaveChangesAsync(cancellationToken);
    }
}
