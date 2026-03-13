using ApplicationCore.Commands.Submissions.ProcessPollExecution;
using ApplicationCore.Domain.Submissions;
using FluentValidation.Results;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
internal sealed class ProcessPollExecutionValidatorTests
{
    private ProcessPollExecutionValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ProcessPollExecutionValidator();
    }

    [Test]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
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

        ValidationResult result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithNullSubmissions_ShouldBeInvalid()
    {
        var command = new ProcessPollExecutionsCommand(null!);

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
