using Algowars.Domain.Problems.Enums;
using Algowars.Domain.Problems.Exceptions;
using Algowars.Domain.Problems.ValueObjects;
using ProblemEntity = Algowars.Domain.Problems.Entities.Problem;

namespace Algowars.Domain.Tests.Problem.Entities;

public class ProblemTests
{
    private static readonly Slug ValidSlug = new("two-sum");
    private static readonly Title ValidTitle = new("Two Sum");
    private static readonly Question ValidQuestion = new(new string('a', Question.MinLength));
    private static readonly Difficulty ValidDifficulty = new(500);
    private static readonly TimeLimit ValidTimeLimit = new(1000);
    private static readonly MemoryLimit ValidMemoryLimit = new(64);

    private static ProblemEntity CreateProblem() =>
        new(ValidSlug, ValidTitle, ValidQuestion, ValidDifficulty, ValidTimeLimit, ValidMemoryLimit);

    [Test]
    public void Archive_SetsStatusToArchived()
    {
        var problem = CreateProblem();

        problem.Archive();

        Assert.That(problem.Status, Is.EqualTo(ProblemStatus.Archived));
    }

    [Test]
    public void Constructor_CreatesInitialDraftVersion()
    {
        var problem = CreateProblem();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(problem.Versions, Has.Count.EqualTo(1));
            Assert.That(problem.Versions.First().VersionNumber, Is.EqualTo(1));
            Assert.That(problem.Versions.First().IsPublished, Is.False);
        }
    }

    [Test]
    public void Constructor_SetsSlug()
    {
        var problem = CreateProblem();

        Assert.That(problem.Slug, Is.EqualTo(ValidSlug));
    }

    [Test]
    public void Constructor_SetsStatusToDraft()
    {
        var problem = CreateProblem();

        Assert.That(problem.Status, Is.EqualTo(ProblemStatus.Draft));
    }

    [Test]
    public void CreateNewVersion_CopiesValuesFromLatestVersion()
    {
        var problem = CreateProblem();

        var newVersion = problem.CreateNewVersion();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(newVersion.Title, Is.EqualTo(ValidTitle));
            Assert.That(newVersion.Question, Is.EqualTo(ValidQuestion));
            Assert.That(newVersion.Difficulty, Is.EqualTo(ValidDifficulty));
            Assert.That(newVersion.TimeLimit, Is.EqualTo(ValidTimeLimit));
            Assert.That(newVersion.MemoryLimit, Is.EqualTo(ValidMemoryLimit));
        }
    }

    [Test]
    public void CreateNewVersion_IncrementsVersionNumber()
    {
        var problem = CreateProblem();

        var newVersion = problem.CreateNewVersion();

        Assert.That(newVersion.VersionNumber, Is.EqualTo(2));
    }

    [Test]
    public void CreateNewVersion_AddsVersionToCollection()
    {
        var problem = CreateProblem();

        problem.CreateNewVersion();

        Assert.That(problem.Versions, Has.Count.EqualTo(2));
    }

    [Test]
    public void CurrentVersion_BeforePublish_IsNull()
    {
        var problem = CreateProblem();

        Assert.That(problem.CurrentVersion, Is.Null);
    }

    [Test]
    public void CurrentVersion_AfterPublish_ReturnsPublishedVersion()
    {
        var problem = CreateProblem();
        var versionId = problem.Versions.First().Id;

        problem.Publish(versionId);

        Assert.That(problem.CurrentVersion, Is.Not.Null);
        Assert.That(problem.CurrentVersion!.Id, Is.EqualTo(versionId));
    }

    [Test]
    public void DraftVersion_ReturnsLatestUnpublishedVersion()
    {
        var problem = CreateProblem();
        var newVersion = problem.CreateNewVersion();

        problem.Publish(problem.Versions.First().Id);

        Assert.That(problem.DraftVersion.Id, Is.EqualTo(newVersion.Id));
    }

    [Test]
    public void Publish_SetsStatusToPublished()
    {
        var problem = CreateProblem();
        var versionId = problem.Versions.First().Id;

        problem.Publish(versionId);

        Assert.That(problem.Status, Is.EqualTo(ProblemStatus.Published));
    }

    [Test]
    public void Publish_SetsVersionPublishedAt()
    {
        var problem = CreateProblem();
        var versionId = problem.Versions.First().Id;
        var before = DateTime.UtcNow;

        problem.Publish(versionId);

        Assert.That(problem.Versions.First().PublishedAt, Is.GreaterThanOrEqualTo(before));
    }

    [Test]
    public void Publish_UnknownVersionId_ThrowsProblemVersionNotFoundException()
    {
        var problem = CreateProblem();

        Assert.Throws<ProblemVersionNotFoundException>(() => problem.Publish(Guid.NewGuid()));
    }

    [Test]
    public void UpdateSlug_ChangesSlug()
    {
        var problem = CreateProblem();
        var newSlug = new Slug("three-sum");

        problem.UpdateSlug(newSlug);

        Assert.That(problem.Slug, Is.EqualTo(newSlug));
    }

    [Test]
    public void UpdateSlug_DoesNotAffectVersions()
    {
        var problem = CreateProblem();
        int versionCount = problem.Versions.Count;

        problem.UpdateSlug(new Slug("three-sum"));

        Assert.That(problem.Versions, Has.Count.EqualTo(versionCount));
    }
}
