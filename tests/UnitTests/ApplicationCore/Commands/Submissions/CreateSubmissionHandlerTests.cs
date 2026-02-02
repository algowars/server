using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using FluentValidation;
using Moq;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
public sealed class CreateSubmissionHandlerTests
{
    private Mock<ISubmissionRepository> _mockSubmissionRepository;
    private Mock<IValidator<CreateSubmissionCommand>> _mockValidator;

    private CreateSubmissionHandler _sut;

    [SetUp]
    public void SetUp()
    {
        _mockSubmissionRepository = new();
        _mockValidator = new();
        _mockValidator = new();

        _mockValidator
            .Setup(v =>
                v.ValidateAsync(It.IsAny<CreateSubmissionCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _sut = new CreateSubmissionHandler(_mockSubmissionRepository.Object, _mockValidator.Object);
    }

    [Test]
    public async Task Handle_creates_submission_successfully()
    {
        _mockSubmissionRepository
            .Setup(a => a.SaveAsync(It.IsAny<SubmissionModel>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateSubmissionCommand(1, "code", Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.EqualTo(Guid.Empty));

            _mockSubmissionRepository.Verify(
                a =>
                    a.SaveAsync(
                        It.Is<SubmissionModel>(s =>
                            s.ProblemSetupId == command.ProblemSetupId
                            && s.Code == command.Code
                            && s.CreatedById == command.CreatedById
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }

    [Test]
    public async Task Handle_returns_result_error_when_exception_occurs()
    {
        _mockSubmissionRepository
            .Setup(a => a.SaveAsync(It.IsAny<SubmissionModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));
        var command = new CreateSubmissionCommand(1, "code", Guid.NewGuid());

        var result = await _sut.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Errors,
                Has.Exactly(1).Matches<string>(e => e.Contains("Database error"))
            );

            _mockSubmissionRepository.Verify(
                a =>
                    a.SaveAsync(
                        It.Is<SubmissionModel>(s =>
                            s.ProblemSetupId == command.ProblemSetupId
                            && s.Code == command.Code
                            && s.CreatedById == command.CreatedById
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}