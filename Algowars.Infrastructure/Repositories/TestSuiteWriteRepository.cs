using Algowars.Domain.TestSuites;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class TestSuiteWriteRepository(AlgowarsDbContext context) : ITestSuiteWriteRepository
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

    public async Task<IReadOnlyList<Guid>> FindTestCaseIdsByProblemSetupIdAsync(
        Guid problemSetupId,
        CancellationToken cancellationToken = default)
    {
        return await context.Problems
            .AsNoTracking()
            .SelectMany(p => p.Setups)
            .Where(s => s.Id == problemSetupId)
            .SelectMany(s => s.TestSuites)
            .SelectMany(ts => ts.TestCases)
            .Select(tc => tc.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, string>> FindExpectedOutputsByTestCaseIdsAsync(
        IEnumerable<Guid> testCaseIds,
        CancellationToken cancellationToken = default)
    {
        var ids = testCaseIds.Distinct().ToArray();
        if (ids.Length == 0)
            return [];

        var rows = await context.TestSuites
            .AsNoTracking()
            .SelectMany(ts => ts.TestCases)
            .Where(tc => ids.Contains(tc.Id))
            .Select(tc => new
            {
                tc.Id,
                ExpectedOutput = string.Join(", ", tc.ExpectedOutputs.Select(output => output.Value))
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Id, x => x.ExpectedOutput);
    }

    public async Task<Guid?> FindPipelineIdByProblemSetupIdAsync(
        Guid problemSetupId,
        CancellationToken cancellationToken = default)
    {
        return await context.Problems
            .AsNoTracking()
            .SelectMany(p => p.Setups)
            .Where(s => s.Id == problemSetupId)
            .Select(s => (Guid?)s.PipelineId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
