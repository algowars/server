namespace ApplicationCore.Dtos.Problems.Tests;

public record TestSuiteDto
{
    public required IEnumerable<TestCaseDto> TestCases { get; init; }
}