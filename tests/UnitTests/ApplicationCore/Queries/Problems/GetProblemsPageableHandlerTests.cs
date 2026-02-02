using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Problems.GetProblemsPageable;
using Ardalis.Result;
using Moq;

namespace UnitTests.ApplicationCore.Queries.Problems;

[TestFixture]
public sealed class GetProblemsPageableHandlerTests
{
    private Mock<IProblemRepository> _repo = null!;
    private GetProblemsPageableHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IProblemRepository>();
        _handler = new GetProblemsPageableHandler(_repo.Object);
    }

    [Test]
    public async Task Handle_returns_success_with_mapped_paginated_result()
    {
        var pagination = new PaginationRequest() { Page = 1, Size = 10 };

        var problems = new PaginatedResult<ProblemModel>
        {
            Results =
            [
                new ProblemModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Two Sum",
                    Slug = "two-sum",
                    Difficulty = 1,
                    Version = 1,
                    Tags =
                    [
                        new TagModel() { Id = 1, Value = "arrays" },
                        new TagModel() { Id = 1, Value = "hashmap" },
                    ],
                    Question = "",
                },
                new ProblemModel()
                {
                    Id = Guid.NewGuid(),
                    Title = "Reverse String",
                    Slug = "reverse-string",
                    Difficulty = 1,
                    Version = 1,
                    Tags = [],
                    Question = "",
                },
            ],
            Total = 2,
            Page = 1,
            Size = 10,
        };

        _repo
            .Setup(r => r.GetProblemsAsync(pagination, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var result = await _handler.Handle(
            new GetProblemsPageableQuery(pagination),
            CancellationToken.None
        );

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Total, Is.EqualTo(2));
            Assert.That(result.Value.Page, Is.EqualTo(1));
            Assert.That(result.Value.Size, Is.EqualTo(10));
            Assert.That(result.Value.Results.Count, Is.EqualTo(2));

            var first = result.Value.Results[0];
            Assert.That(first.Title, Is.EqualTo("Two Sum"));
            Assert.That(first.Slug, Is.EqualTo("two-sum"));
            Assert.That(first.Tags, Is.EquivalentTo(["arrays", "hashmap"]));

            var second = result.Value.Results[1];
            Assert.That(second.Tags, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_returns_success_with_empty_results_when_repository_returns_none()
    {
        var pagination = new PaginationRequest() { Page = 1, Size = 10 };

        var problems = new PaginatedResult<ProblemModel>
        {
            Results = [],
            Total = 0,
            Page = 1,
            Size = 10,
        };

        _repo
            .Setup(r => r.GetProblemsAsync(pagination, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var result = await _handler.Handle(
            new GetProblemsPageableQuery(pagination),
            CancellationToken.None
        );

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
            Assert.That(result.Value.Results, Is.Empty);
            Assert.That(result.Value.Total, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task Handle_returns_error_when_repository_throws()
    {
        var pagination = new PaginationRequest() { Page = 1, Size = 10 };

        _repo
            .Setup(r => r.GetProblemsAsync(pagination, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db failure"));

        var result = await _handler.Handle(
            new GetProblemsPageableQuery(pagination),
            CancellationToken.None
        );

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Some.EqualTo("db failure"));
        });
    }
}