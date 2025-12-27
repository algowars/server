using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.TestSuites;

namespace ApplicationCore.Domain.Problems.ProblemSetups;

public sealed class ProblemSetup
{
    public required int Id { get; init; }

    public required Guid ProblemId { get; init; }

    public ProblemModel? Problem { get; init; }

    public required string InitialCode { get; init; }

    public string? FunctionName { get; init; }

    public LanguageVersion? LanguageVersion { get; init; }

    public int Version { get; init; }

    public HarnessTemplate? HarnessTemplate { get; init; }

    public IEnumerable<TestSuite> TestSuites { get; init; } = [];
}
