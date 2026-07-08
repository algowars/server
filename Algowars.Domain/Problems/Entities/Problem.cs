using Algowars.Domain.Problems.Enums;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Entities;

public sealed class Problem : AggregateRoot
{
    public Problem(Slug slug, Title title, Question question, Difficulty difficulty, TimeLimit timeLimit, MemoryLimit memoryLimit)
    {
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Question = question ?? throw new ArgumentNullException(nameof(question));
        Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
        TimeLimit = timeLimit ?? throw new ArgumentNullException(nameof(timeLimit));
        MemoryLimit = memoryLimit ?? throw new ArgumentNullException(nameof(memoryLimit));
        Status = ProblemStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = ProblemStatus.Archived;
    }

    public void Publish()
    {
        Status = ProblemStatus.Published;
    }

    public void UpdateContent(Title title, Question question, Difficulty difficulty, TimeLimit timeLimit, MemoryLimit memoryLimit)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Question = question ?? throw new ArgumentNullException(nameof(question));
        Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
        TimeLimit = timeLimit ?? throw new ArgumentNullException(nameof(timeLimit));
        MemoryLimit = memoryLimit ?? throw new ArgumentNullException(nameof(memoryLimit));

        _history.Add(new ProblemHistory(Title, Question, Difficulty, TimeLimit, MemoryLimit));
    }

    public void UpdateSlug(Slug slug)
    {
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
    }

    public ProblemSetup AddSetup(Guid languageVersionId, string initialCode, string functionName)
    {
        var setup = new ProblemSetup(languageVersionId, initialCode, functionName);
        _setups.Add(setup);
        return setup;
    }

    private Problem() { }

    public IEnumerable<Guid> AvailableLanguageVersionIds() => _setups.Select(setup => setup.LanguageVersionId);

    public ProblemSetup? FindSetupByLanguageVersionId(Guid languageVersionId) => _setups.SingleOrDefault(setup => setup.LanguageVersionId == languageVersionId);

    public Slug Slug { get; private set; }
    public Title Title { get; private set; }
    public Question Question { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public TimeLimit TimeLimit { get; private set; }
    public MemoryLimit MemoryLimit { get; private set; }
    public ProblemStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<ProblemHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<ProblemSetup> Setups => _setups.AsReadOnly();

    private readonly List<ProblemHistory> _history = [];
    private readonly List<ProblemSetup> _setups = [];
}
 