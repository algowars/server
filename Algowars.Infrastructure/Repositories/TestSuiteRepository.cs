using Algowars.Domain.TestSuites;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class TestSuiteRepository(AlgowarsDbContext context) : ITestSuiteRepository
{
    public async Task AddAsync(TestSuite testSuite, CancellationToken cancellationToken = default)
    {
        await context.TestSuites.AddAsync(testSuite, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TestSuite testSuite, CancellationToken cancellationToken = default)
    {
        context.TestSuites.Update(testSuite);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TestSuite?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TestSuites
            .Include(s => s.TestCases)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
