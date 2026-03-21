using ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
internal class IncrementSubmissionOutboxesHandlerTests
{
    private IncrementSubmissionOutboxesHandler _sut;
    private Mock<ISubmissionRepository> _mockSubmissionRepository;
    private Mock<IValidator<IncrementSubmissionOutboxesCommand>> _mockValidator;

    [SetUp]
    public void SetUp()
    {
        _mockSubmissionRepository = new();

        _mockValidator = new();
        _mockValidator
            .Setup(v =>
                v.ValidateAsync(
                    It.IsAny<IncrementSubmissionOutboxesCommand>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new ValidationResult());

        _sut = new IncrementSubmissionOutboxesHandler(
            _mockSubmissionRepository.Object,
            _mockValidator.Object
        );
    }

    [Test]
    public async Task Handle_ShouldIncrementOutboxesCountSuccessfully()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.IncrementOutboxesCountAsync(
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var command = new IncrementSubmissionOutboxesCommand(
            [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
            DateTime.UtcNow
        );
        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        _mockSubmissionRepository.Verify(
            r =>
                r.IncrementOutboxesCountAsync(
                    command.OutboxIds,
                    command.Timestamp,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task Handle_RepositoryError_ShouldReturnErrorResult()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.IncrementOutboxesCountAsync(
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception("Database error"));
        var command = new IncrementSubmissionOutboxesCommand(
            [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
            DateTime.UtcNow
        );
        var result = await _sut.Handle(command, CancellationToken.None);
        Assert.That(result.IsError, Is.True);
    }
}