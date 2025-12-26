namespace ApplicationCore.Domain.Problems.TestSuites;

public sealed class TestSuite : BaseAuditableModel<int>
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    public TestSuiteType TestSuiteType { get; init; }

    public required IEnumerable<TestCase> TestCases { get; init; }
}
