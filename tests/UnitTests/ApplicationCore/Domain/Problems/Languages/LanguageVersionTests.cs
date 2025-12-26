using ApplicationCore.Domain.Problems.Languages;

namespace UnitTests.ApplicationCore.Domain.Problems.Languages;

[TestFixture]
public sealed class LanguageVersionTests
{
    [Test]
    public void Creating_language_version_with_required_version_sets_value()
    {
        var language = new ProgrammingLanguage
        {
            Id = 1,
            Name = "C#",
            IsArchived = false
        };

        var languageVersion = new LanguageVersion
        {
            Version = "12",
            ProgrammingLanguageId = language.Id,
            ProgrammingLanguage = language
        };

        Assert.That(languageVersion.Version, Is.EqualTo("12"));
    }

    [Test]
    public void Optional_initial_code_can_be_set()
    {
        var language = new ProgrammingLanguage
        {
            Id = 1,
            Name = "Python",
            IsArchived = false
        };

        const string code = "print('hello world')";

        var languageVersion = new LanguageVersion
        {
            Version = "3.12",
            InitialCode = code,
            ProgrammingLanguageId = language.Id,
            ProgrammingLanguage = language
        };

        Assert.That(languageVersion.InitialCode, Is.EqualTo(code));
    }

    [Test]
    public void Programming_language_relationship_is_set_correctly()
    {
        var language = new ProgrammingLanguage
        {
            Id = 5,
            Name = "Java",
            IsArchived = false
        };

        var languageVersion = new LanguageVersion
        {
            Version = "21",
            ProgrammingLanguageId = 5,
            ProgrammingLanguage = language
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(languageVersion.ProgrammingLanguageId, Is.EqualTo(5));
            Assert.That(languageVersion.ProgrammingLanguage, Is.EqualTo(language));
        }
    }

    [Test]
    public void Accessing_required_version_without_initialization_throws()
    {
        var languageVersion = new LanguageVersion
        {
            Version = "1.1.1"
        };

        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = languageVersion.Version;
        });
    }
}
