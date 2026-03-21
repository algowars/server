using ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;
using FluentValidation.Results;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
internal class IncrementSubmissionOutboxesValidatorTests
{
    private IncrementSubmissionOutboxesValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new IncrementSubmissionOutboxesValidator();
    }

    [Test]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var command = new IncrementSubmissionOutboxesCommand(
            [Guid.NewGuid()],
            DateTime.UtcNow.AddSeconds(-1)
        );

        ValidationResult result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithEmptyOutboxIds_ShouldBeInvalid()
    {
        var command = new IncrementSubmissionOutboxesCommand([], DateTime.UtcNow.AddSeconds(-1));

        ValidationResult result = _validator.Validate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Exactly(1).Matches<ValidationFailure>(e => e.PropertyName == "OutboxIds")
            );
        }
    }

    [Test]
    public void Validate_WithFutureTimestamp_ShouldBeInvalid()
    {
        var command = new IncrementSubmissionOutboxesCommand(
            [Guid.NewGuid()],
            DateTime.UtcNow.AddMinutes(5)
        );

        ValidationResult result = _validator.Validate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Exactly(1).Matches<ValidationFailure>(e => e.PropertyName == "Timestamp")
            );
        }
    }
}