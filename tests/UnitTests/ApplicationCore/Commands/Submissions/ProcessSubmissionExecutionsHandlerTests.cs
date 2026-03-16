using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
internal class ProcessSubmissionExecutionsHandlerTests
{
    private Mock<ISubmissionRepository> _mockSubmissionRepository = null!;
    private Mock<IValidator<ProcessSubmissionExecutionsCommand>> _mockValidator = null!;

    private ProcessSubmissionExecutionsHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockSubmissionRepository = new Mock<ISubmissionRepository>();
        _mockValidator = new Mock<IValidator<ProcessSubmissionExecutionsCommand>>();

        _mockValidator
            .Setup(v =>
                v.ValidateAsync(
                    It.IsAny<ProcessSubmissionExecutionsCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new ValidationResult());

        _sut = new ProcessSubmissionExecutionsHandler(
            _mockSubmissionRepository.Object,
            _mockValidator.Object
        );
    }

    [Test]
    public async Task Handle_processes_submissions_successfully()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.ProcessSubmissionInitializationAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var submissions = new List<SubmissionModel>
        {
            new() { Id = Guid.NewGuid(), CreatedById = Guid.NewGuid() },
        };
        var command = new ProcessSubmissionExecutionsCommand(submissions);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            _mockSubmissionRepository.Verify(
                r =>
                    r.ProcessSubmissionInitializationAsync(
                        submissions,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }

    [Test]
    public async Task Handle_returns_error_when_repository_throws()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.ProcessSubmissionInitializationAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception("Database error"));

        var submissions = new List<SubmissionModel>
        {
            new() { Id = Guid.NewGuid(), CreatedById = Guid.NewGuid() },
        };
        var command = new ProcessSubmissionExecutionsCommand(submissions);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsError(), Is.True);
            Assert.That(result.Errors, Has.Some.EqualTo("Database error"));
        }
    }

    [Test]
    public async Task Handle_returns_invalid_when_validation_fails()
    {
        _mockValidator
            .Setup(v =>
                v.ValidateAsync(
                    It.IsAny<ProcessSubmissionExecutionsCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new ValidationResult(
                    [new ValidationFailure("Submissions", "Submissions must not be null.")]
                )
            );

        var command = new ProcessSubmissionExecutionsCommand(null!);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.That(result.IsInvalid(), Is.True);
    }
}
