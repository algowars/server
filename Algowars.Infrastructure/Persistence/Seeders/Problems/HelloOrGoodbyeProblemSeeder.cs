using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Languages.ValueObjects;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Domain.TestSuites.Enums;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders.Problems;

internal sealed class HelloOrGoodbyeProblemSeeder(AlgowarsDbContext context) : ISeeder
{
    private const string ProblemSlug = "hello-or-goodbye";

    private static readonly string[] Tags = ["math", "conditionals"];

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
            (jsVersion.Id, "function helloOrGoodbye(n) {\n    \n}", "helloOrGoodbye"),
            (tsVersion.Id, "function helloOrGoodbye(n: number): string {\n    \n}", "helloOrGoodbye"),
            (pyVersion.Id, "def hello_or_goodbye(n: int) -> str:\n    pass", "hello_or_goodbye"),
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
                new Title("Hello or Goodbye"),
                new Question("Given an integer n, return \"Hello\" if n is even, or \"Goodbye\" if n is odd."),
                new Difficulty(100),
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

        HashSet<Guid> existingVersionIds = await context.Set<ProblemSetup>()
            .AsNoTracking()
            .Where(s => EF.Property<Guid>(s, "problem_id") == existingId.Value)
            .Select(s => s.LanguageVersionId)
            .ToHashSetAsync(cancellationToken);

        List<(Guid versionId, string code, string funcName)> missing =
            desiredSetups.Where(d => !existingVersionIds.Contains(d.versionId)).ToList();

        if (missing.Count > 0)
        {
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

            List<Guid> setupIds = await context.Set<ProblemSetup>()
                .AsNoTracking()
                .Where(s => EF.Property<Guid>(s, "problem_id") == problemId)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            foreach (Guid setupId in setupIds)
            {
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
        TestSuite suite = new("Hello or Goodbye - Sample Cases", TestSuiteType.Sample);

        TestCase case1 = suite.AddTestCase("Even number");
        case1.AddInput("4", "integer");
        case1.AddExpectedOutput("Hello", "string");

        TestCase case2 = suite.AddTestCase("Odd number");
        case2.AddInput("7", "integer");
        case2.AddExpectedOutput("Goodbye", "string");

        TestCase case3 = suite.AddTestCase("Zero");
        case3.AddInput("0", "integer");
        case3.AddExpectedOutput("Hello", "string");

        return suite;
    }

    private static TestSuite BuildHiddenSuite()
    {
        TestSuite suite = new("Hello or Goodbye - Hidden Cases", TestSuiteType.Hidden);

        TestCase case1 = suite.AddTestCase("Large even");
        case1.AddInput("1000000", "integer");
        case1.AddExpectedOutput("Hello", "string");

        TestCase case2 = suite.AddTestCase("Large odd");
        case2.AddInput("999999", "integer");
        case2.AddExpectedOutput("Goodbye", "string");

        TestCase case3 = suite.AddTestCase("Negative even");
        case3.AddInput("-2", "integer");
        case3.AddExpectedOutput("Hello", "string");

        TestCase case4 = suite.AddTestCase("Negative odd");
        case4.AddInput("-3", "integer");
        case4.AddExpectedOutput("Goodbye", "string");

        return suite;
    }
}
