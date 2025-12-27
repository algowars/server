using ApplicationCore.Dtos.Problems.Tests;

namespace ApplicationCore.Dtos.Problems;

public record ProblemSetupDto
{
    public int Id { get; init; }

    public int Version { get; init; }

    public required string InitialCode { get; init; }

    public int LanguageVersionId { get; init; }

    public required IEnumerable<TestSuiteDto> TestSuites { get; init; }
}
