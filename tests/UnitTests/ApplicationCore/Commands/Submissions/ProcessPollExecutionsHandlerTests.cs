using ApplicationCore.Commands.Submissions.ProcessPollExecution;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
internal sealed class ProcessPollExecutionsHandlerTests
{
    private ProcessPollExecutionsHandler _sut;
    private Mock<ISubmissionRepository> _mockSubmissionRepository;
    private Mock<IValidator<ProcessPollExecutionsCommand>> _mockValidator;

    [SetUp]
    public void SetUp()
    {
        _mockSubmissionRepository = new();

        _mockValidator = new();
        _mockValidator
            .Setup(v =>
                v.ValidateAsync(
                    It.IsAny<ProcessPollExecutionsCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new ValidationResult());

        _sut = new ProcessPollExecutionsHandler(
            _mockSubmissionRepository.Object,
            _mockValidator.Object
        );
    }

    [Test]
    public async Task Handle_ShouldProcessPollExecutionSuccessfully()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.ProcessPollExecutionAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var submissions = new List<SubmissionModel>
        {
            new SubmissionModel
            {
                Id = Guid.NewGuid(),
                ProblemSetupId = 1,
                CreatedById = Guid.NewGuid(),
            },
        };

        var command = new ProcessPollExecutionsCommand(submissions);
        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        _mockSubmissionRepository.Verify(
            r =>
                r.ProcessPollExecutionAsync(
                    command.Submissions,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task Handle_RepositoryError_ShouldPropagateException()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.ProcessPollExecutionAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception("Database error"));

        var command = new ProcessPollExecutionsCommand([]);

        Assert.ThrowsAsync<Exception>(() => _sut.Handle(command, CancellationToken.None));
    }
}
