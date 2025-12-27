using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Problems.GetAvailableLanguages;
using Moq;

namespace UnitTests.ApplicationCore.Queries.Problems;

[TestFixture]
public sealed class GetAvailableLanguagesHandlerTests
{
    private Mock<IProblemRepository> _problemRepository = null!;
    private GetAvailableLanguagesHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _problemRepository = new Mock<IProblemRepository>();
        _handler = new GetAvailableLanguagesHandler(_problemRepository.Object);
    }

    [Test]
    public async Task Handle_returns_languages_with_versions()
    {
        var languages = new[]
        {
            new ProgrammingLanguage
            {
                Id = 1,
                Name = "C#",
                IsArchived = false,
                Versions =
                [
                    new LanguageVersion
                    {
                        Id = 1,
                        Version = "10",
                        InitialCode = "code10",
                    },
                    new LanguageVersion
                    {
                        Id = 2,
                        Version = "11",
                        InitialCode = "code11",
                    },
                ],
            },
        };

        _problemRepository
            .Setup(r => r.GetAllLanguagesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(languages);

        var result = await _handler.Handle(
            new GetAvailableLanguagesQuery(),
            CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);

        var dto = result.Value.Single();

        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(1));
            Assert.That(dto.Name, Is.EqualTo("C#"));
            Assert.That(dto.Versions, Has.Count.EqualTo(2));
            Assert.That(dto.Versions.Select(v => v.Version), Is.EqualTo(new[] { "10", "11" }));
        });
    }

    [Test]
    public async Task Handle_returns_empty_when_no_languages_exist()
    {
        _problemRepository
            .Setup(r => r.GetAllLanguagesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(
            new GetAvailableLanguagesQuery(),
            CancellationToken.None
        );

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_returns_error_when_exception_thrown()
    {
        _problemRepository
            .Setup(r => r.GetAllLanguagesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db failure"));

        var result = await _handler.Handle(
            new GetAvailableLanguagesQuery(),
            CancellationToken.None
        );

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Some.EqualTo("db failure"));
        });
    }
}
