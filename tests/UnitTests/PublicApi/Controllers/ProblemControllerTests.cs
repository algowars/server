using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PublicApi.Controllers;

namespace UnitTests.PublicApi.Controllers;

[TestFixture]
public sealed class ProblemControllerTests
{
    private Mock<IProblemAppService> _problemAppService = null!;
    private Mock<IAccountAppService> _accountAppService = null!;
    private ProblemController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _problemAppService = new Mock<IProblemAppService>();
        _accountAppService = new Mock<IAccountAppService>();
        _sut = new ProblemController(_problemAppService.Object, _accountAppService.Object);
    }

    [Test]
    public async Task GetBySlugAsync_returns_ok_when_problem_exists()
    {
        var problem = new ProblemDto
        {
            Id = Guid.NewGuid(),
            Title = "Two Sum",
            Slug = "two-sum",
            Question = "Find two numbers",
            Difficulty = 1,
            Version = 1,
            Tags = [],
            AvailableLanguages = [],
        };

        _problemAppService
            .Setup(service => service.GetProblemBySlugAsync("two-sum", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(problem));

        var result = await _sut.GetBySlugAsync("two-sum", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That(((OkObjectResult)result).Value, Is.EqualTo(problem));
    }

    [Test]
    public async Task GetBySlugAsync_returns_bad_request_when_slug_is_missing()
    {
        var result = await _sut.GetBySlugAsync("", CancellationToken.None);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Slug is required."));
        _problemAppService.Verify(
            service => service.GetProblemBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
