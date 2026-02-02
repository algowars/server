using ApplicationCore.Commands.Submissions.CreateSubmission;
using Moq;

namespace UnitTests.ApplicationCore.Commands.Submissions;

[TestFixture]
public sealed class CreateSubmissionValidatorTests
{
    private CreateSubmissionValidator _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new CreateSubmissionValidator();
    }

    [Test]
    public void Should_Pass_For_Valid_Command()
    {
        var command = new CreateSubmissionCommand(1, "print('Hello, World!')", Guid.NewGuid());
        var result = _sut.Validate(command);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Code_Should_Fail_If_Empty()
    {
        var command = new CreateSubmissionCommand(1, "", Guid.NewGuid());
        var result = _sut.Validate(command);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void ProblemSetupId_Should_Fail_If_Non_Positive()
    {
        var command = new CreateSubmissionCommand(0, "print('Hello, World!')", Guid.NewGuid());
        var result = _sut.Validate(command);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
        });
    }

    [Test]
    public void CreatedById_Should_Fail_If_Empty()
    {
        var command = new CreateSubmissionCommand(1, "print('Hello, World!')", Guid.Empty);
        var result = _sut.Validate(command);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
        });
    }
}