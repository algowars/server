using Algowars.Domain.Problems.Enums;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.SeedWork;
using Algowars.Domain.Users.Entities;

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

    public void AddTag(ProblemTag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (!_tags.Contains(tag))
            _tags.Add(tag);
    }

    public void RemoveTag(ProblemTag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        _tags.Remove(tag);
    }

    public void SetCreatedBy(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        CreatedById = userId;
    }

    private Problem()
    {
        Slug = null!;
        Title = null!;
        Question = null!;
        Difficulty = null!;
        TimeLimit = null!;
        MemoryLimit = null!;
    }

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
    public Guid? CreatedById { get; private set; }
    public User? CreatedBy { get; private set; }

    public IReadOnlyCollection<ProblemHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<ProblemSetup> Setups => _setups.AsReadOnly();
    public IReadOnlyCollection<ProblemTag> Tags => _tags.AsReadOnly();

    private readonly List<ProblemHistory> _history = [];
    private readonly List<ProblemSetup> _setups = [];
    private readonly List<ProblemTag> _tags = [];
}