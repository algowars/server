using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Services;
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
}