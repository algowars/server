using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Problems.GetProblemBySlug;
using Ardalis.Result;
using Mapster;
using Moq;

namespace UnitTests.ApplicationCore.Queries.Problems;

[TestFixture]
public sealed class GetProblemBySlugHandlerTests
{
    private Mock<IProblemRepository> _problemRepository = null!;
    private GetProblemBySlugHandler _handler = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(ProblemModel).Assembly);
    }

    [SetUp]
    public void SetUp()
    {
        _problemRepository = new Mock<IProblemRepository>();
        _handler = new GetProblemBySlugHandler(_problemRepository.Object);
    }

    [Test]
    public async Task Handle_returns_problem_dto_when_problem_exists()
    {
        var language = new ProgrammingLanguage
        {
            Id = 1,
            Name = "C#",
            IsArchived = false,
        };

        var version = new LanguageVersion
        {
            Id = 1,
            Version = "10",
            ProgrammingLanguage = language,
        };

        var problem = new ProblemModel()
        {
            Id = Guid.NewGuid(),
            Title = "Two Sum",
            Slug = "two-sum",
            Question = "Find two numbers",
            Tags = [],
            Difficulty = 1,
            Version = 1,
            ProblemSetups =
            [
                new ProblemSetup
                {
                    Id = 1,
                    ProblemId = Guid.NewGuid(),
                    InitialCode = "",
                    LanguageVersion = version,
                },
            ],
        };

        _problemRepository
            .Setup(r => r.GetProblemBySlugAsync("two-sum", It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        var result = await _handler.Handle(
            new GetProblemBySlugQuery("two-sum"),
            CancellationToken.None
        );

        Assert.That(result.IsSuccess, Is.True);

        var dto = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo("Two Sum"));
            Assert.That(dto.Slug, Is.EqualTo("two-sum"));
            Assert.That(dto.AvailableLanguages.Count(), Is.EqualTo(1));
            Assert.That(dto.AvailableLanguages.Single().Name, Is.EqualTo("C#"));
            Assert.That(
                dto.AvailableLanguages.Single().Versions.Single().Version,
                Is.EqualTo("10")
            );
        });
    }

    [Test]
    public async Task Handle_returns_not_found_when_problem_missing()
    {
        _problemRepository
            .Setup(r => r.GetProblemBySlugAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProblemModel?)null);

        var result = await _handler.Handle(
            new GetProblemBySlugQuery("missing"),
            CancellationToken.None
        );

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Handle_returns_error_when_exception_thrown()
    {
        _problemRepository
            .Setup(r => r.GetProblemBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _handler.Handle(
            new GetProblemBySlugQuery("two-sum"),
            CancellationToken.None
        );

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Some.EqualTo("db error"));
        });
    }
}
