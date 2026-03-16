using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Submissions.GetSubmissionOutboxes;
using Ardalis.Result;
using Moq;

namespace UnitTests.ApplicationCore.Queries.Submissions;

[TestFixture]
public sealed class GetSubmissionOutboxesHandlerTests
{
    private Mock<ISubmissionRepository> _mockRepository = null!;
    private GetSubmissionOutboxesHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<ISubmissionRepository>();
        _handler = new GetSubmissionOutboxesHandler(_mockRepository.Object);
    }

    [Test]
    public async Task Handle_returns_outboxes_when_repository_succeeds()
    {
        var submissionId = Guid.NewGuid();
        var outboxes = new List<SubmissionOutboxModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SubmissionId = submissionId,
                Submission = new SubmissionModel
                {
                    Id = submissionId,
                    CreatedById = Guid.NewGuid(),
                },
                Type = SubmissionOutboxType.Initialized,
                Status = SubmissionOutboxStatus.Pending,
            },
        };

        _mockRepository
            .Setup(r => r.GetSubmissionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(outboxes);

        var query = new GetSubmissionOutboxesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Exactly(1).Items);
            Assert.That(result.Value.First().SubmissionId, Is.EqualTo(submissionId));
        });
    }

    [Test]
    public async Task Handle_returns_empty_collection_when_no_outboxes_exist()
    {
        _mockRepository
            .Setup(r => r.GetSubmissionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetSubmissionOutboxesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_returns_error_when_exception_is_thrown()
    {
        _mockRepository
            .Setup(r => r.GetSubmissionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetSubmissionOutboxesQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Error));
            Assert.That(result.Errors, Has.Some.EqualTo("Database error"));
        });
    }
}
