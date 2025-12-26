using ApplicationCore.Domain.Problems.Languages;

namespace UnitTests.ApplicationCore.Domain.Problems.Languages;

[TestFixture]
public sealed class ProgrammingLanguageTests
{
    [Test]
    public void Creating_programming_language_with_required_properties_sets_values()
    {
        var language = new ProgrammingLanguage
        {
            Name = "C#",
            IsArchived = false
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(language.Name, Is.EqualTo("C#"));
            Assert.That(language.IsArchived, Is.False);
        }
    }

    [Test]
    public void Versions_is_initialized_empty_by_default()
    {
        var language = new ProgrammingLanguage
        {
            Name = "Python",
            IsArchived = false
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(language.Versions, Is.Not.Null);
            Assert.That(language.Versions, Is.Empty);
        }
    }

    [Test]
    public void Versions_can_be_assigned()
    {
        var version = new LanguageVersion
        {
            Id = 1,
            Version = "3.12",
            ProgrammingLanguageId = 1
        };

        var language = new ProgrammingLanguage
        {
            Name = "Python",
            IsArchived = false,
            Versions = { version }
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(language.Versions, Has.Count.EqualTo(1));
            Assert.That(language.Versions.Single(), Is.EqualTo(version));
        }
    }

    [Test]
    public void Auditable_fields_can_be_set()
    {
        var createdOn = DateTime.UtcNow;
        var modifiedOn = DateTime.UtcNow.AddMinutes(1);
        var deletedOn = DateTime.UtcNow.AddMinutes(2);
        var userId = Guid.NewGuid();

        var language = new ProgrammingLanguage
        {
            Name = "Java",
            IsArchived = false,
            CreatedOn = createdOn,
            CreatedById = userId,
            LastModifiedOn = modifiedOn,
            LastModifiedById = userId,
            DeletedOn = deletedOn
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(language.CreatedOn, Is.EqualTo(createdOn));
            Assert.That(language.CreatedById, Is.EqualTo(userId));
            Assert.That(language.LastModifiedOn, Is.EqualTo(modifiedOn));
            Assert.That(language.LastModifiedById, Is.EqualTo(userId));
            Assert.That(language.DeletedOn, Is.EqualTo(deletedOn));
        }
    }
}
