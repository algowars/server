using Algowars.Domain.TestSuites;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Domain.TestSuites.Enums;
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

    public async Task<IReadOnlyList<Guid>> FindPublicTestCaseIdsByProblemSetupIdAsync(
        Guid problemSetupId,
        CancellationToken cancellationToken = default)
    {
        return await context.Problems
            .AsNoTracking()
            .SelectMany(p => p.Setups)
            .Where(s => s.Id == problemSetupId)
            .SelectMany(s => s.TestSuites)
            .Where(ts => ts.Type == TestSuiteType.Sample)
            .SelectMany(ts => ts.TestCases)
            .Select(tc => tc.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> FindRandomHiddenTestCaseIdsByProblemSetupIdAsync(
        Guid problemSetupId,
        int maxCases,
        CancellationToken cancellationToken = default)
    {
        var hiddenIds = await context.Problems
            .AsNoTracking()
            .SelectMany(p => p.Setups)
            .Where(s => s.Id == problemSetupId)
            .SelectMany(s => s.TestSuites)
            .Where(ts => ts.Type == TestSuiteType.Hidden)
            .SelectMany(ts => ts.TestCases)
            .Select(tc => tc.Id)
            .ToListAsync(cancellationToken);

        if (hiddenIds.Count == 0)
            return [];

        int take = maxCases <= 0 ? hiddenIds.Count : Math.Min(maxCases, hiddenIds.Count);

        return [.. hiddenIds
            .OrderBy(_ => Guid.NewGuid())
            .Take(take)];
    }

    public async Task<IReadOnlyList<Guid>> CreateAdHocTestCasesAsync(
        IReadOnlyCollection<IReadOnlyCollection<string>> customTestCaseInputs,
        CancellationToken cancellationToken = default)
    {
        if (customTestCaseInputs.Count == 0)
            return [];

        var suite = new TestSuite($"AdHoc Submission Suite {Guid.NewGuid():N}", TestSuiteType.Sample);

        int index = 1;
        foreach (var customInputs in customTestCaseInputs)
        {
            var testCase = suite.AddTestCase($"Custom {index}");
            foreach (string input in customInputs)
            {
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                testCase.AddInput(input, "json");
            }

            index++;
        }

        await context.TestSuites.AddAsync(suite, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return [.. suite.TestCases.Select(tc => tc.Id)];
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
