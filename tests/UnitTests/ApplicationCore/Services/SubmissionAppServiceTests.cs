using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;
using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Queries.Submissions.GetSubmissionOutboxes;
using ApplicationCore.Services;
using Ardalis.Result;
using MediatR;
using Moq;

namespace UnitTests.ApplicationCore.Services;

[TestFixture]
public sealed class SubmissionAppServiceTests
{
    private Mock<IMediator> _mockMediator;
    private SubmissionAppService _sut;

    [SetUp]
    public void SetUp()
    {
        _mockMediator = new();
        _sut = new SubmissionAppService(_mockMediator.Object);
    }

    [Test]
    public void CreateAsync_sends_CreateSubmissionCommand_via_mediator()
    {
        int problemSetupId = 1;
        string code = "sample code";
        var createdById = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var expectedResult = Guid.NewGuid();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateSubmissionCommand>(), cancellationToken))
            .ReturnsAsync(expectedResult);

        var result = _sut.CreateAsync(problemSetupId, code, createdById, cancellationToken).Result;

        Assert.That(result.Value, Is.EqualTo(expectedResult));

        _mockMediator.Verify(
            m =>
                m.Send(
                    It.Is<CreateSubmissionCommand>(cmd =>
                        cmd.ProblemSetupId == problemSetupId
                        && cmd.Code == code
                        && cmd.CreatedById == createdById
                    ),
                    cancellationToken
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GetSubmissionOutboxesAsync_sends_GetSubmissionOutboxesQuery_via_mediator()
    {
        var cancellationToken = CancellationToken.None;
        var submissionId = Guid.NewGuid();
        var expectedOutboxes = new List<SubmissionOutboxModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SubmissionId = submissionId,
                Submission = new SubmissionModel { Id = submissionId, CreatedById = Guid.NewGuid() },
                Type = SubmissionOutboxType.Initialized,
                Status = SubmissionOutboxStatus.Pending,
            },
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetSubmissionOutboxesQuery>(), cancellationToken))
            .ReturnsAsync(Result.Success<IEnumerable<SubmissionOutboxModel>>(expectedOutboxes));

        var result = await _sut.GetSubmissionOutboxesAsync(cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Exactly(1).Items);
        }

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetSubmissionOutboxesQuery>(), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task IncrementOutboxesCountAsync_sends_IncrementSubmissionOutboxesCommand_via_mediator()
    {
        var cancellationToken = CancellationToken.None;
        var outboxIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var timestamp = DateTime.UtcNow;

        _mockMediator
            .Setup(m =>
                m.Send(It.IsAny<IncrementSubmissionOutboxesCommand>(), cancellationToken)
            )
            .ReturnsAsync(Result.Success());

        var result = await _sut.IncrementOutboxesCountAsync(outboxIds, timestamp, cancellationToken);

        Assert.That(result.IsSuccess, Is.True);

        _mockMediator.Verify(
            m =>
                m.Send(
                    It.Is<IncrementSubmissionOutboxesCommand>(cmd =>
                        cmd.OutboxIds == outboxIds && cmd.Timestamp == timestamp
                    ),
                    cancellationToken
                ),
            Times.Once
        );
    }

    [Test]
    public async Task ProcessSubmissionExecutionAsync_sends_ProcessSubmissionExecutionsCommand_via_mediator()
    {
        var cancellationToken = CancellationToken.None;
        var submissions = new List<SubmissionModel>
        {
            new() { Id = Guid.NewGuid(), CreatedById = Guid.NewGuid() },
        };

        _mockMediator
            .Setup(m =>
                m.Send(It.IsAny<ProcessSubmissionExecutionsCommand>(), cancellationToken)
            )
            .ReturnsAsync(Result.Success());

        var result = await _sut.ProcessSubmissionExecutionAsync(submissions, cancellationToken);

        Assert.That(result.IsSuccess, Is.True);

        _mockMediator.Verify(
            m =>
                m.Send(
                    It.Is<ProcessSubmissionExecutionsCommand>(cmd => cmd.Submissions == submissions),
                    cancellationToken
                ),
            Times.Once
        );
    }
}