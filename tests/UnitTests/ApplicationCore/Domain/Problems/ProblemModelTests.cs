using System;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;

namespace UnitTests.ApplicationCore.Domain.Problems;

[TestFixture]
public sealed class ProblemModelTests
{
    private static ProblemModel CreateProblem(IEnumerable<ProblemSetupModel>? setups = null)
    {
        return new ProblemModel
        {
            Title = "Sample Problem",
            Slug = "sample-problem",
            Question = "Solve the problem",
            Tags = [],
            Difficulty = 2,
            Status = ProblemStatus.Draft,
            Version = 1,
            ProblemSetups = setups?.ToList() ?? [],
        };
    }

    [Test]
    public void GetAvailableLanguages_returns_empty_when_no_problem_setups_exist()
    {
        var problem = CreateProblem();

        var result = problem.GetAvailableLanguages();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAvailableLanguages_returns_single_language_with_multiple_versions()
    {
        var language = new ProgrammingLanguage
        {
            Id = 1,
            Name = "C#",
            IsArchived = false,
        };

        var v10 = new LanguageVersion
        {
            Id = 10,
            Version = "10",
            ProgrammingLanguage = language,
        };

        var v11 = new LanguageVersion
        {
            Id = 11,
            Version = "11",
            ProgrammingLanguage = language,
        };

        var setups = new[]
        {
            new ProblemSetupModel
            {
                Id = 1,
                ProblemId = Guid.NewGuid(),
                LanguageVersionId = v10.Id,
                InitialCode = "",
                LanguageVersion = v10,
            },
            new ProblemSetupModel
            {
                Id = 2,
                ProblemId = Guid.NewGuid(),
                LanguageVersionId = v10.Id,
                InitialCode = "",
                LanguageVersion = v11,
            },
        };

        var problem = CreateProblem(setups);

        var languages = problem.GetAvailableLanguages().ToList();

        IEnumerable<string> expected = ["10", "11"];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(languages, Has.Count.EqualTo(1));
            Assert.That(languages[0].Id, Is.EqualTo(1));
            Assert.That(languages[0].Name, Is.EqualTo("C#"));
            Assert.That(languages[0].IsArchived, Is.False);
            Assert.That(languages[0].Versions.Select(v => v.Version), Is.EqualTo(expected));
        }
    }

    [Test]
    public void GetAvailableLanguages_deduplicates_language_versions_by_id()
    {
        var language = new ProgrammingLanguage
        {
            Id = 2,
            Name = "Python",
            IsArchived = false,
        };

        var version = new LanguageVersion
        {
            Id = 1,
            Version = "3.11",
            ProgrammingLanguage = language,
        };

        var setups = new[]
        {
            new ProblemSetupModel
            {
                Id = 1,
                ProblemId = Guid.NewGuid(),
                LanguageVersionId = version.Id,
                InitialCode = "",
                LanguageVersion = version,
            },
            new ProblemSetupModel
            {
                Id = 2,
                ProblemId = Guid.NewGuid(),
                LanguageVersionId = version.Id,
                InitialCode = "",
                LanguageVersion = version,
            },
        };

        var problem = CreateProblem(setups);

        var languages = problem.GetAvailableLanguages().ToList();

        Assert.That(languages.Single().Versions, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetAvailableLanguages_ignores_setups_without_language_version_or_language()
    {
        var version = new LanguageVersion
        {
            Id = 1,
            Version = "1.0",
            ProgrammingLanguage = null!,
        };

        var setups = new[]
        {
            new ProblemSetupModel
            {
                Id = 1,
                ProblemId = Guid.NewGuid(),
                LanguageVersionId = version.Id,
                InitialCode = "",
                LanguageVersion = null,
            },
            new ProblemSetupModel
            {
                Id = 2,
                ProblemId = Guid.NewGuid(),
                InitialCode = "",
                LanguageVersion = version,
                LanguageVersionId = version.Id,
            },
        };

        var problem = CreateProblem(setups);

        var result = problem.GetAvailableLanguages();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAvailableLanguages_orders_versions_by_version_value()
    {
        var language = new ProgrammingLanguage
        {
            Id = 3,
            Name = "Java",
            IsArchived = false,
        };

        var v21 = new LanguageVersion
        {
            Id = 2,
            Version = "21",
            ProgrammingLanguage = language,
        };

        var v17 = new LanguageVersion
        {
            Id = 1,
            Version = "17",
            ProgrammingLanguage = language,
        };

        var setups = new[]
        {
            new ProblemSetupModel
            {
                Id = 1,
                ProblemId = Guid.NewGuid(),
                InitialCode = "",
                LanguageVersion = v21,
                LanguageVersionId = v21.Id,
            },
            new ProblemSetupModel
            {
                Id = 2,
                ProblemId = Guid.NewGuid(),
                InitialCode = "",
                LanguageVersion = v17,
                LanguageVersionId = v21.Id,
            },
        };

        var problem = CreateProblem(setups);

        var versions = problem
            .GetAvailableLanguages()
            .Single()
            .Versions.Select(v => v.Version)
            .ToList();

        IEnumerable<string> expected = ["17", "21"];

        Assert.That(versions, Is.EqualTo(expected));
    }

    [Test]
    public void GetAvailableLanguages_deduplicates_languages_by_id()
    {
        var lang1 = new ProgrammingLanguage
        {
            Id = 1,
            Name = "C#",
            IsArchived = false,
        };
        var lang2 = new ProgrammingLanguage
        {
            Id = 1,
            Name = "C#",
            IsArchived = true,
        };

        var v1 = new LanguageVersion
        {
            Id = 1,
            Version = "10",
            ProgrammingLanguage = lang1,
        };
        var v2 = new LanguageVersion
        {
            Id = 2,
            Version = "11",
            ProgrammingLanguage = lang2,
        };

        var setups = new[]
        {
            new ProblemSetupModel
            {
                Id = 1,
                ProblemId = Guid.NewGuid(),
                InitialCode = "",
                LanguageVersion = v1,
                LanguageVersionId = v1.Id,
            },
            new ProblemSetupModel
            {
                Id = 2,
                ProblemId = Guid.NewGuid(),
                InitialCode = "",
                LanguageVersion = v2,
                LanguageVersionId = v2.Id,
            },
        };

        var problem = CreateProblem(setups);

        var result = problem.GetAvailableLanguages().Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.IsArchived, Is.False);
            Assert.That(result.Versions, Has.Count.EqualTo(1));
        }
    }
}
