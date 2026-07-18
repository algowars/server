using Algowars.Domain.Problems;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class ProblemRepository(AlgowarsDbContext context) : IProblemRepository
{
    public async Task AddAsync(Problem entity, CancellationToken cancellationToken = default)
    {
        await context.Problems.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Problem entity, CancellationToken cancellationToken = default)
    {
        context.Problems.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Problem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Problems
            .Include(p => p.Setups)
                .ThenInclude(s => s.TestSuites)
                    .ThenInclude(ts => ts.TestCases)
                        .ThenInclude(tc => tc.Inputs)
            .Include(p => p.Setups)
                .ThenInclude(s => s.TestSuites)
                    .ThenInclude(ts => ts.TestCases)
                        .ThenInclude(tc => tc.ExpectedOutputs)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Problem?> FindBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => await context.Problems
            .Include(p => p.Setups)
                .ThenInclude(s => s.TestSuites)
                    .ThenInclude(ts => ts.TestCases)
                        .ThenInclude(tc => tc.Inputs)
            .Include(p => p.Setups)
                .ThenInclude(s => s.TestSuites)
                    .ThenInclude(ts => ts.TestCases)
                        .ThenInclude(tc => tc.ExpectedOutputs)
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

    public async Task<Problem?> FindBySetupIdAsync(Guid setupId, CancellationToken cancellationToken = default)
        => await context.Problems
            .Include(p => p.Setups)
                .ThenInclude(s => s.TestSuites)
                    .ThenInclude(ts => ts.TestCases)
                        .ThenInclude(tc => tc.Inputs)
            .Include(p => p.Setups)
                .ThenInclude(s => s.TestSuites)
                    .ThenInclude(ts => ts.TestCases)
                        .ThenInclude(tc => tc.ExpectedOutputs)
            .Where(p => p.Setups.Any(s => s.Id == setupId))
            .FirstOrDefaultAsync(cancellationToken);
}
