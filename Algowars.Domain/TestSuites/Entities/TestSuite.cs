using Algowars.Domain.SeedWork;
using Algowars.Domain.TestSuites.Enums;

namespace Algowars.Domain.TestSuites.Entities;

public sealed class TestSuite : AggregateRoot
{
    public TestSuite(string name, TestSuiteType type)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name must not be empty.", nameof(name));

        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public TestCase AddTestCase(string name, string? description = null)
    {
        var testCase = new TestCase(name, description);
        _testCases.Add(testCase);
        return testCase;
    }

    private TestSuite() { }

    public string Name { get; private set; } = null!;
    public TestSuiteType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<TestCase> TestCases => _testCases.AsReadOnly();

    private readonly List<TestCase> _testCases = [];
}
