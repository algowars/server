using Algowars.Domain.Problems.Exceptions;
using Algowars.Domain.Problems.ValueObjects;
using ProblemEntity = Algowars.Domain.Problems.Entities.Problem;

namespace Algowars.Domain.Tests.Problem.Entities;

public class ProblemVersionTests
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
    public void AddCodeTemplate_AfterPublish_ThrowsProblemVersionImmutableException()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);

        Assert.Throws<ProblemVersionImmutableException>(() =>
            version.AddCodeTemplate(Guid.NewGuid(), "starter", "wrapper"));
    }

    [Test]
    public void AddCodeTemplate_BeforePublish_AddsToCollection()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        version.AddCodeTemplate(Guid.NewGuid(), "starter", "wrapper");

        Assert.That(version.CodeTemplates, Has.Count.EqualTo(1));
    }

    [Test]
    public void AddCodeTemplate_SetsProperties()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        var languageId = Guid.NewGuid();

        version.AddCodeTemplate(languageId, "starter", "wrapper");
        var template = version.CodeTemplates.First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(template.LanguageId, Is.EqualTo(languageId));
            Assert.That(template.StarterCode, Is.EqualTo("starter"));
            Assert.That(template.WrapperCode, Is.EqualTo("wrapper"));
        }
    }

    [Test]
    public void AddExample_AfterPublish_Succeeds()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);

        Assert.That(() => version.AddExample("1", "2"), Throws.Nothing);
    }

    [Test]
    public void AddExample_SetsProperties()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        version.AddExample("[1,2]", "3", "add them");
        var example = version.Examples.First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(example.Input, Is.EqualTo("[1,2]"));
            Assert.That(example.Output, Is.EqualTo("3"));
            Assert.That(example.Explanation, Is.EqualTo("add them"));
        }
    }

    [Test]
    public void AddExample_WithoutExplanation_ExplanationIsNull()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        version.AddExample("[1,2]", "3");

        Assert.That(version.Examples.First().Explanation, Is.Null);
    }

    [Test]
    public void AddTestCase_AfterPublish_ThrowsProblemVersionImmutableException()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);

        Assert.Throws<ProblemVersionImmutableException>(() =>
            version.AddTestCase("input", "expected"));
    }

    [Test]
    public void AddTestCase_BeforePublish_AddsToCollection()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        version.AddTestCase("input", "expected");

        Assert.That(version.TestCases, Has.Count.EqualTo(1));
    }

    [Test]
    public void AddTestCase_DefaultsToHidden()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        version.AddTestCase("input", "expected");

        Assert.That(version.TestCases.First().IsHidden, Is.True);
    }

    [Test]
    public void AddTestCase_SetsProperties()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        version.AddTestCase("input", "expected", isHidden: false);
        var testCase = version.TestCases.First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(testCase.Input, Is.EqualTo("input"));
            Assert.That(testCase.ExpectedOutput, Is.EqualTo("expected"));
            Assert.That(testCase.IsHidden, Is.False);
        }
    }

    [Test]
    public void Constructor_SetsInitialProperties()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(version.VersionNumber, Is.EqualTo(1));
            Assert.That(version.Title, Is.EqualTo(ValidTitle));
            Assert.That(version.Question, Is.EqualTo(ValidQuestion));
            Assert.That(version.Difficulty, Is.EqualTo(ValidDifficulty));
            Assert.That(version.TimeLimit, Is.EqualTo(ValidTimeLimit));
            Assert.That(version.MemoryLimit, Is.EqualTo(ValidMemoryLimit));
        }
    }

    [Test]
    public void IsPublished_BeforePublish_IsFalse()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        Assert.That(version.IsPublished, Is.False);
    }

    [Test]
    public void IsPublished_AfterPublish_IsTrue()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();

        problem.Publish(version.Id);

        Assert.That(version.IsPublished, Is.True);
    }

    [Test]
    public void UpdateDifficulty_AfterPublish_Succeeds()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);
        var newDifficulty = new Difficulty(1500);

        version.UpdateDifficulty(newDifficulty);

        Assert.That(version.Difficulty, Is.EqualTo(newDifficulty));
    }

    [Test]
    public void UpdateDifficulty_BeforePublish_UpdatesDifficulty()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        var newDifficulty = new Difficulty(1500);

        version.UpdateDifficulty(newDifficulty);

        Assert.That(version.Difficulty, Is.EqualTo(newDifficulty));
    }

    [Test]
    public void UpdateMemoryLimit_AfterPublish_ThrowsProblemVersionImmutableException()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);

        Assert.Throws<ProblemVersionImmutableException>(() =>
            version.UpdateMemoryLimit(new MemoryLimit(128)));
    }

    [Test]
    public void UpdateMemoryLimit_BeforePublish_UpdatesMemoryLimit()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        var newLimit = new MemoryLimit(128);

        version.UpdateMemoryLimit(newLimit);

        Assert.That(version.MemoryLimit, Is.EqualTo(newLimit));
    }

    [Test]
    public void UpdateQuestion_AfterPublish_UpdatesQuestion()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);
        var newQuestion = new Question(new string('b', Question.MinLength));

        version.UpdateQuestion(newQuestion);

        Assert.That(version.Question, Is.EqualTo(newQuestion));
    }

    [Test]
    public void UpdateTimeLimit_AfterPublish_ThrowsProblemVersionImmutableException()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);

        Assert.Throws<ProblemVersionImmutableException>(() =>
            version.UpdateTimeLimit(new TimeLimit(2000)));
    }

    [Test]
    public void UpdateTimeLimit_BeforePublish_UpdatesTimeLimit()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        var newLimit = new TimeLimit(2000);

        version.UpdateTimeLimit(newLimit);

        Assert.That(version.TimeLimit, Is.EqualTo(newLimit));
    }

    [Test]
    public void UpdateTitle_AfterPublish_UpdatesTitle()
    {
        var problem = CreateProblem();
        var version = problem.Versions.First();
        problem.Publish(version.Id);
        var newTitle = new Title("Two Sum Fixed");

        version.UpdateTitle(newTitle);

        Assert.That(version.Title, Is.EqualTo(newTitle));
    }
}
