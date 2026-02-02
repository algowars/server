namespace ApplicationCore.Domain.Problems.TestSuites;

public sealed class TestSuiteModel : BaseAuditableModel<int>
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    public TestSuiteType TestSuiteType { get; init; }

    public required IEnumerable<TestCaseModel> TestCases { get; init; }
}