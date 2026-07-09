using Algowars.Domain.Submissions;
using Algowars.Domain.Submissions.Entities;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class SubmissionWriteRepository(AlgowarsDbContext context) : ISubmissionWriteRepository
{
    public async Task AddAsync(Submission entity, CancellationToken cancellationToken = default)
    {
        await context.Submissions.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Submission?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Submissions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Submission entity, CancellationToken cancellationToken = default)
    {
        context.Submissions.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}