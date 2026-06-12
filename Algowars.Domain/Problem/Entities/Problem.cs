using Algowars.Domain.Problem.Enums;
using Algowars.Domain.Problem.Exceptions;
using Algowars.Domain.Problem.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Entities;

public sealed class Problem : AggregateRoot
{
    public Problem(Slug slug, Title title, Question question, Difficulty difficulty, TimeLimit timeLimit, MemoryLimit memoryLimit)
    {
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Status = ProblemStatus.Draft;
        _versions.Add(new ProblemVersion(1, title, question, difficulty, timeLimit, memoryLimit));
    }

    public void Archive()
    {
        Status = ProblemStatus.Archived;
    }

    public ProblemVersion CreateNewVersion()
    {
        var latest = CurrentVersion ?? _versions.Last();
        int nextNumber = _versions.Max(v => v.VersionNumber) + 1;
        var newVersion = new ProblemVersion(nextNumber, latest.Title, latest.Question, latest.Difficulty, latest.TimeLimit, latest.MemoryLimit);
        _versions.Add(newVersion);
        return newVersion;
    }

    public void Publish(Guid versionId)
    {
        var version = _versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw new ProblemVersionNotFoundException(versionId);

        version.Publish();
        Status = ProblemStatus.Published;
    }

    public void UpdateSlug(Slug slug)
    {
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
    }

    private Problem() { }

    public ProblemVersion? CurrentVersion => _versions
        .Where(v => v.IsPublished)
        .OrderByDescending(v => v.VersionNumber)
        .FirstOrDefault();

    public ProblemVersion DraftVersion => _versions
        .OrderByDescending(v => v.VersionNumber)
        .First(v => !v.IsPublished);

    public Slug Slug { get; private set; } = null!;
    public ProblemStatus Status { get; private set; }
    public IReadOnlyCollection<ProblemVersion> Versions => _versions.AsReadOnly();

    private readonly List<ProblemVersion> _versions = [];
}
