namespace ApplicationCore.Domain.Problems.TestSuites;

public sealed class TestCaseModel
{
    public required int Id { get; init; }

    public IEnumerable<TestCaseInputParamModel> Inputs { get; init; }

    public required TestCaseExpectedOutputModel ExpectedOutput { get; init; }
}