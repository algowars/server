using Algowars.Domain.Problems.Exceptions;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Entities;

public sealed class ProblemVersion : Entity
{
    internal ProblemVersion(int versionNumber, Title title, Question question, Difficulty difficulty, TimeLimit timeLimit, MemoryLimit memoryLimit)
    {
        VersionNumber = versionNumber;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Question = question ?? throw new ArgumentNullException(nameof(question));
        Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
        TimeLimit = timeLimit ?? throw new ArgumentNullException(nameof(timeLimit));
        MemoryLimit = memoryLimit ?? throw new ArgumentNullException(nameof(memoryLimit));
    }

    public void AddCodeTemplate(Guid languageId, string starterCode, string wrapperCode)
    {
        if (IsPublished)
            throw new ProblemVersionImmutableException();

        _codeTemplates.Add(new CodeTemplate(languageId, starterCode, wrapperCode));
    }

    public void AddExample(string input, string output, string? explanation = null)
    {
        _examples.Add(new Example(input, output, explanation));
    }

    public void AddTestCase(string input, string expectedOutput, bool isHidden = true)
    {
        if (IsPublished)
            throw new ProblemVersionImmutableException();

        _testCases.Add(new TestCase(input, expectedOutput, isHidden));
    }

    public void UpdateDifficulty(Difficulty difficulty)
    {
        Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
    }

    public void UpdateMemoryLimit(MemoryLimit memoryLimit)
    {
        if (IsPublished)
            throw new ProblemVersionImmutableException();

        MemoryLimit = memoryLimit ?? throw new ArgumentNullException(nameof(memoryLimit));
    }

    public void UpdateQuestion(Question question)
    {
        Question = question ?? throw new ArgumentNullException(nameof(question));
    }

    public void UpdateTimeLimit(TimeLimit timeLimit)
    {
        if (IsPublished)
            throw new ProblemVersionImmutableException();

        TimeLimit = timeLimit ?? throw new ArgumentNullException(nameof(timeLimit));
    }

    public void UpdateTitle(Title title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    internal void Publish()
    {
        PublishedAt = DateTime.UtcNow;
    }

    private ProblemVersion() { }

    public IReadOnlyCollection<CodeTemplate> CodeTemplates => _codeTemplates.AsReadOnly();
    public Difficulty Difficulty { get; private set; } = null!;
    public IReadOnlyCollection<Example> Examples => _examples.AsReadOnly();
    public bool IsPublished => PublishedAt.HasValue;
    public MemoryLimit MemoryLimit { get; private set; } = null!;
    public DateTime? PublishedAt { get; private set; }
    public Question Question { get; private set; } = null!;
    public IReadOnlyCollection<TestCase> TestCases => _testCases.AsReadOnly();
    public TimeLimit TimeLimit { get; private set; } = null!;
    public Title Title { get; private set; } = null!;
    public int VersionNumber { get; private set; }

    private readonly List<CodeTemplate> _codeTemplates = [];
    private readonly List<Example> _examples = [];
    private readonly List<TestCase> _testCases = [];
}
