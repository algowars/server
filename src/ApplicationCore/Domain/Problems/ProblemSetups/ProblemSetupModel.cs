using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.TestSuites;

namespace ApplicationCore.Domain.Problems.ProblemSetups;

public sealed class ProblemSetupModel
{
    public required int Id { get; init; }

    public required Guid ProblemId { get; init; }

    public ProblemModel? Problem { get; init; }

    public required string InitialCode { get; init; }

    public string? FunctionName { get; init; }

    public required int LanguageVersionId { get; init; }

    public LanguageVersion? LanguageVersion { get; init; }

    public int Version { get; init; }

    public HarnessTemplate? HarnessTemplate { get; init; }

    public IEnumerable<TestSuiteModel> TestSuites { get; init; } = [];
}
