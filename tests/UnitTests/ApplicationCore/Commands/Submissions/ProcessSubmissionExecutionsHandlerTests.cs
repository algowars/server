using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Interfaces.Repositories;
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
}