using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Languages.ValueObjects;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Domain.TestSuites.Enums;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders.Problems;

internal sealed class TwoSumProblemSeeder(AlgowarsDbContext context) : ISeeder
{
    private const string ProblemSlug = "two-sum";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LanguageVersionEntry jsVersion = await GetVersionAsync("javascript", cancellationToken);
        LanguageVersionEntry tsVersion = await GetVersionAsync("typescript", cancellationToken);
        LanguageVersionEntry pyVersion = await GetVersionAsync("python", cancellationToken);

        Problem? problem = await context.Problems
            .IgnoreQueryFilters()
            .AsSplitQuery()
            .Include(p => p.Setups)
                .ThenInclude(s => s.TestSuites)
            .FirstOrDefaultAsync(p => p.Slug == new Slug(ProblemSlug), cancellationToken);

        if (problem is null)
        {
            problem = new Problem(
                new Slug(ProblemSlug),
                new Title("Two Sum"),
                new Question("Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target. You may assume that each input would have exactly one solution, and you may not use the same element twice."),
                new Difficulty(500),
                new TimeLimit(1000),
                new MemoryLimit(64));

            problem.Publish();
            context.Problems.Add(problem);
        }
        else
        {
            problem.UpdateContent(
                new Title("Two Sum"),
                new Question("Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target. You may assume that each input would have exactly one solution, and you may not use the same element twice."),
                new Difficulty(500),
                new TimeLimit(1000),
                new MemoryLimit(64));
        }

        await context.SaveChangesAsync(cancellationToken);

        await UpsertSetupAsync(problem, jsVersion.Id, "function twoSum(nums, target) {\n    \n}", "twoSum", cancellationToken);
        await UpsertSetupAsync(problem, tsVersion.Id, "function twoSum(nums: number[], target: number): number[] {\n    \n}", "twoSum", cancellationToken);
        await UpsertSetupAsync(problem, pyVersion.Id, "def two_sum(nums: list[int], target: int) -> list[int]:\n    pass", "two_sum", cancellationToken);

        await UpsertTestSuiteAsync(problem, BuildSampleSuite(), cancellationToken);
        await UpsertTestSuiteAsync(problem, BuildHiddenSuite(), cancellationToken);
    }

    private async Task UpsertSetupAsync(Problem problem, Guid languageVersionId, string initialCode, string functionName, CancellationToken cancellationToken)
    {
        bool exists = problem.Setups.Any(s => s.LanguageVersionId == languageVersionId);
        if (!exists)
        {
            problem.AddSetup(languageVersionId, initialCode, functionName);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task UpsertTestSuiteAsync(Problem problem, TestSuite desired, CancellationToken cancellationToken)
    {
        TestSuite? existing = await context.TestSuites
            .FirstOrDefaultAsync(t => t.Name == desired.Name, cancellationToken);

        if (existing is null)
        {
            context.TestSuites.Add(desired);
            await context.SaveChangesAsync(cancellationToken);
            existing = desired;
        }

        foreach (ProblemSetup setup in problem.Setups)
        {
            ProblemSetup tracked = await context.Set<ProblemSetup>()
                .Include(s => s.TestSuites)
                .FirstAsync(s => s.Id == setup.Id, cancellationToken);

            bool linked = tracked.TestSuites.Any(t => t.Id == existing.Id);
            if (!linked)
            {
                ((List<TestSuite>)tracked.TestSuites).Add(existing);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task<LanguageVersionEntry> GetVersionAsync(string slug, CancellationToken cancellationToken)
        => await context.Languages
            .Where(l => l.Slug == new LanguageSlug(slug))
            .SelectMany(l => l.Versions)
            .FirstAsync(cancellationToken);

    private static TestSuite BuildSampleSuite()
    {
        TestSuite suite = new("Two Sum - Sample Cases", TestSuiteType.Sample);

        TestCase case1 = suite.AddTestCase("Example 1");
        case1.AddInput("[2,7,11,15]", "integer_array");
        case1.AddInput("9", "integer");
        case1.AddExpectedOutput("[0,1]", "integer_array");

        TestCase case2 = suite.AddTestCase("Example 2");
        case2.AddInput("[3,2,4]", "integer_array");
        case2.AddInput("6", "integer");
        case2.AddExpectedOutput("[1,2]", "integer_array");

        return suite;
    }

    private static TestSuite BuildHiddenSuite()
    {
        TestSuite suite = new("Two Sum - Hidden Cases", TestSuiteType.Hidden);

        TestCase case1 = suite.AddTestCase("Hidden 1");
        case1.AddInput("[1,2,3,4,5]", "integer_array");
        case1.AddInput("9", "integer");
        case1.AddExpectedOutput("[3,4]", "integer_array");

        TestCase case2 = suite.AddTestCase("Hidden 2");
        case2.AddInput("[0,4,3,0]", "integer_array");
        case2.AddInput("0", "integer");
        case2.AddExpectedOutput("[0,3]", "integer_array");

        return suite;
    }
}
