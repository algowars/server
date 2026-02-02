namespace ApplicationCore.Domain.Problems.TestSuites;

public sealed class TestCaseModel
{
    public required int Id { get; init; }

    public TestCaseType TestCaseType { get; init; }

    public required string Input { get; init; }

    public required string ExpectedOutput { get; init; }
}