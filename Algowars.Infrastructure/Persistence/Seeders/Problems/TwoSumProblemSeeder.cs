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

    private static readonly string[] Tags = ["array", "hash-table"];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        Guid problemId = await EnsureProblemWithSetupsAsync(cancellationToken);

        await EnsureTestSuitesLinkedAsync(problemId, cancellationToken);

        await EnsureTagsLinkedAsync(problemId, cancellationToken);
    }

    private async Task<Guid> EnsureProblemWithSetupsAsync(CancellationToken cancellationToken)
    {
        LanguageVersionEntry jsVersion = await GetVersionAsync("javascript", cancellationToken);
        LanguageVersionEntry tsVersion = await GetVersionAsync("typescript", cancellationToken);
        LanguageVersionEntry pyVersion = await GetVersionAsync("python", cancellationToken);

        context.ChangeTracker.Clear();

        (Guid versionId, string code, string funcName)[] desiredSetups =
        [
            (jsVersion.Id, "function twoSum(nums, target) {\n    \n}", "twoSum"),
            (tsVersion.Id, "function twoSum(nums: number[], target: number): number[] {\n    \n}", "twoSum"),
            (pyVersion.Id, "def two_sum(nums: list[int], target: int) -> list[int]:\n    pass", "two_sum"),
        ];

        Guid? existingId = await context.Problems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.Slug.Value == ProblemSlug)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingId is null)
        {
            Problem problem = new(
                new Slug(ProblemSlug),
                new Title("Two Sum"),
                new Question("Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target. You may assume that each input would have exactly one solution, and you may not use the same element twice."),
                new Difficulty(500),
                new TimeLimit(1000),
                new MemoryLimit(64));

            problem.Publish();

            foreach ((Guid versionId, string code, string funcName) in desiredSetups)
                problem.AddSetup(versionId, code, funcName);

            context.Problems.Add(problem);
            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();
            return problem.Id;
        }

        // Problem exists — add only missing setups
        HashSet<Guid> existingVersionIds = await context.Set<ProblemSetup>()
            .AsNoTracking()
            .Where(s => EF.Property<Guid>(s, "problem_id") == existingId.Value)
            .Select(s => s.LanguageVersionId)
            .ToHashSetAsync(cancellationToken);

        List<(Guid versionId, string code, string funcName)> missing =
            desiredSetups.Where(d => !existingVersionIds.Contains(d.versionId)).ToList();

        if (missing.Count > 0)
        {
            // Bypass EF change tracking entirely for setup inserts to avoid
            // DbUpdateConcurrencyException caused by PropertyAccessMode.Field
            // collections (_history, _setups) being in an unresolvable state.
            foreach ((Guid versionId, string code, string funcName) in missing)
            {
                await context.Database.ExecuteSqlAsync(
                    $"""
                    INSERT INTO problem_setups (id, problem_id, language_version_id, initial_code, function_name)
                    VALUES ({Guid.NewGuid()}, {existingId.Value}, {versionId}, {code}, {funcName})
                    """,
                    cancellationToken);
            }
        }

        return existingId.Value;
    }

    private async Task EnsureTestSuitesLinkedAsync(Guid problemId, CancellationToken cancellationToken)
    {
        TestSuite[] desired = [BuildSampleSuite(), BuildHiddenSuite()];

        foreach (TestSuite suite in desired)
        {
            // Upsert test suite by name
            TestSuite? existing = await context.TestSuites
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Name == suite.Name, cancellationToken);

            Guid suiteId;
            if (existing is null)
            {
                context.TestSuites.Add(suite);
                await context.SaveChangesAsync(cancellationToken);
                context.ChangeTracker.Clear();
                suiteId = suite.Id;
            }
            else
            {
                suiteId = existing.Id;
            }

            // Get all setup IDs for this problem
            List<Guid> setupIds = await context.Set<ProblemSetup>()
                .AsNoTracking()
                .Where(s => EF.Property<Guid>(s, "problem_id") == problemId)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            foreach (Guid setupId in setupIds)
            {
                // Insert join row only if not already linked (raw SQL to avoid field-backed collection issues)
                await context.Database.ExecuteSqlAsync(
                    $"""
                    INSERT INTO problem_setup_test_suites (problem_setup_id, test_suite_id)
                    VALUES ({setupId}, {suiteId})
                    ON CONFLICT DO NOTHING
                    """,
                    cancellationToken);
            }
        }
    }

    private async Task EnsureTagsLinkedAsync(Guid problemId, CancellationToken cancellationToken)
    {
        foreach (string tagName in Tags)
        {
            await context.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO tags (id, name)
                VALUES ({Guid.NewGuid()}, {tagName})
                ON CONFLICT (name) DO NOTHING
                """,
                cancellationToken);

            await context.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO problem_tags ("ProblemsId", "TagsId")
                SELECT {problemId}, id FROM tags WHERE name = {tagName}
                ON CONFLICT DO NOTHING
                """,
                cancellationToken);
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

