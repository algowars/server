namespace ApplicationCore.Dtos.Problems.Tests;

public record TestCaseDto
{
    public string Input { get; init; } = string.Empty;
    public string ExpectedOutput { get; init; } = string.Empty;
}
