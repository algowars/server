using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

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
    public async Task Handle_processes_initialization_successfully()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.ProcessSubmissionInitializationAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var submissions = new[]
        {
            new SubmissionModel
            {
                Id = Guid.NewGuid(),
                ProblemSetupId = 1,
                CreatedOn = DateTime.UtcNow,
                CreatedById = Guid.NewGuid(),
            },
        };

        var command = new ProcessSubmissionExecutionsCommand(submissions);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);

        _mockSubmissionRepository.Verify(
            r =>
                r.ProcessSubmissionInitializationAsync(
                    command.submissions,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task Handle_returns_result_error_when_exception_occurs()
    {
        _mockSubmissionRepository
            .Setup(r =>
                r.ProcessSubmissionInitializationAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception("Database error"));

        var submissions = new[]
        {
            new SubmissionModel
            {
                Id = Guid.NewGuid(),
                ProblemSetupId = 1,
                CreatedOn = DateTime.UtcNow,
                CreatedById = Guid.NewGuid(),
            },
        };

        var command = new ProcessSubmissionExecutionsCommand(submissions);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Errors,
                Has.Exactly(1).Matches<string>(e => e.Contains("Database error"))
            );
        }

        _mockSubmissionRepository.Verify(
            r =>
                r.ProcessSubmissionInitializationAsync(
                    command.submissions,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
