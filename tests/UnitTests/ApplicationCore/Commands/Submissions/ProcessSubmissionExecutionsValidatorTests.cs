using ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;
using ApplicationCore.Domain.Submissions;
using FluentValidation.Results;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
internal class ProcessSubmissionExecutionsValidatorTests
{
    private ProcessSubmissionExecutionsValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ProcessSubmissionExecutionsValidator();
    }

    [Test]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        var submissions = new List<SubmissionModel>
        {
            new() { Id = Guid.NewGuid(), CreatedById = Guid.NewGuid() },
        };
        var command = new ProcessSubmissionExecutionsCommand(submissions);

        ValidationResult result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithEmptySubmissions_ShouldBeValid()
    {
        var command = new ProcessSubmissionExecutionsCommand([]);

        ValidationResult result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithNullSubmissions_ShouldBeInvalid()
    {
        var command = new ProcessSubmissionExecutionsCommand(null!);

        ValidationResult result = _validator.Validate(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(
                result.Errors,
                Has.Exactly(1).Matches<ValidationFailure>(e => e.PropertyName == "Submissions")
            );
        }
    }
}
