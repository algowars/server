using Algowars.Domain.SeedWork;
using Algowars.Domain.TestSuites.Entities;

namespace Algowars.Domain.Problems.Entities;

public sealed class ProblemSetup : Entity
{
    internal ProblemSetup(Guid languageVersionId, string initialCode, string functionName)
    {
        LanguageVersionId = languageVersionId != Guid.Empty
            ? languageVersionId
            : throw new ArgumentException("Language version id must not be empty.", nameof(languageVersionId));

        InitialCode = !string.IsNullOrWhiteSpace(initialCode)
            ? initialCode
            : throw new ArgumentException("Initial code must not be empty.", nameof(initialCode));

        FunctionName = !string.IsNullOrWhiteSpace(functionName)
            ? functionName
            : throw new ArgumentException("Function name must not be empty.", nameof(functionName));
    }

    private ProblemSetup() { }

    public Guid LanguageVersionId { get; private set; }
    public string InitialCode { get; private set; } = null!;
    public string FunctionName { get; private set; } = null!;

    public IReadOnlyCollection<TestSuite> TestSuites => _testSuites.AsReadOnly();

    private readonly List<TestSuite> _testSuites = [];
}
