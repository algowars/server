using Algowars.Domain.SubmissionJobs;
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
        context.SubmissionJobs.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
