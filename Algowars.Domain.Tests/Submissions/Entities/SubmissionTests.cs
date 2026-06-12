using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Enums;
using Algowars.Domain.Submissions.Exceptions;
using Algowars.Domain.Submissions.ValueObjects;

namespace Algowars.Domain.Tests.Submissions.Entities;

public class SubmissionTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProblemVersionId = Guid.NewGuid();
    private static readonly Guid LanguageVersionId = Guid.NewGuid();
    private static readonly SourceCode ValidSourceCode = new("int main() {}");
    private static readonly Guid TestCaseId1 = Guid.NewGuid();
    private static readonly Guid TestCaseId2 = Guid.NewGuid();

    private static Submission CreateSubmission(
        SubmissionType type = SubmissionType.Submit,
        IEnumerable<Guid>? testCaseIds = null) =>
        new(UserId, ProblemVersionId, LanguageVersionId, type, ValidSourceCode,
            testCaseIds ?? [TestCaseId1, TestCaseId2]);

    [Test]
    public void Complete_AllAccepted_SetsStatusToAccepted()
    {
        var submission = CreateSubmission();
        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted);
        submission.UpdateResult(TestCaseId2, SubmissionResultStatus.Accepted);

        submission.Complete();

        Assert.That(submission.Status, Is.EqualTo(SubmissionStatus.Accepted));
    }

    [Test]
    public void Complete_AnyNonAccepted_SetsStatusToWrongAnswer()
    {
        var submission = CreateSubmission();
        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted);
        submission.UpdateResult(TestCaseId2, SubmissionResultStatus.WrongAnswer);

        submission.Complete();

        Assert.That(submission.Status, Is.EqualTo(SubmissionStatus.WrongAnswer));
    }

    [TestCase(SubmissionResultStatus.TimeLimitExceeded)]
    [TestCase(SubmissionResultStatus.MemoryLimitExceeded)]
    [TestCase(SubmissionResultStatus.RuntimeError)]
    [TestCase(SubmissionResultStatus.CompileError)]
    public void Complete_AnyFailureStatus_SetsStatusToWrongAnswer(SubmissionResultStatus failureStatus)
    {
        var submission = CreateSubmission();
        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted);
        submission.UpdateResult(TestCaseId2, failureStatus);

        submission.Complete();

        Assert.That(submission.Status, Is.EqualTo(SubmissionStatus.WrongAnswer));
    }

    [Test]
    public void Complete_WithPendingResult_ThrowsSubmissionNotCompleteException()
    {
        var submission = CreateSubmission();
        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted);

        Assert.Throws<SubmissionNotCompleteException>(() => submission.Complete());
    }

    [Test]
    public void Complete_WithProcessingResult_ThrowsSubmissionNotCompleteException()
    {
        var submission = CreateSubmission();
        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted);
        submission.UpdateResult(TestCaseId2, SubmissionResultStatus.Processing);

        Assert.Throws<SubmissionNotCompleteException>(() => submission.Complete());
    }

    [Test]
    public void Constructor_SetsInitialProperties()
    {
        var submission = CreateSubmission(SubmissionType.Run);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(submission.UserId, Is.EqualTo(UserId));
            Assert.That(submission.ProblemVersionId, Is.EqualTo(ProblemVersionId));
            Assert.That(submission.LanguageVersionId, Is.EqualTo(LanguageVersionId));
            Assert.That(submission.Type, Is.EqualTo(SubmissionType.Run));
            Assert.That(submission.SourceCode, Is.EqualTo(ValidSourceCode));
            Assert.That(submission.Status, Is.EqualTo(SubmissionStatus.Queued));
        }
    }

    [Test]
    public void Constructor_CreatesResultPerTestCaseId()
    {
        var submission = CreateSubmission();

        Assert.That(submission.Results, Has.Count.EqualTo(2));
    }

    [Test]
    public void Constructor_AllResultsInitiallyPending()
    {
        var submission = CreateSubmission();

        Assert.That(submission.Results.All(r => r.Status == SubmissionResultStatus.Pending), Is.True);
    }

    [Test]
    public void Constructor_WithNoTestCases_ResultsIsEmpty()
    {
        var submission = CreateSubmission(testCaseIds: []);

        Assert.That(submission.Results, Is.Empty);
    }

    [Test]
    public void StartRunning_FromQueued_SetsStatusToRunning()
    {
        var submission = CreateSubmission();

        submission.StartRunning();

        Assert.That(submission.Status, Is.EqualTo(SubmissionStatus.Running));
    }

    [Test]
    public void StartRunning_WhenAlreadyRunning_ThrowsInvalidSubmissionStateException()
    {
        var submission = CreateSubmission();
        submission.StartRunning();

        Assert.Throws<InvalidSubmissionStateException>(() => submission.StartRunning());
    }

    [Test]
    public void UpdateResult_UnknownTestCaseId_ThrowsSubmissionResultNotFoundException()
    {
        var submission = CreateSubmission();

        Assert.Throws<SubmissionResultNotFoundException>(() =>
            submission.UpdateResult(Guid.NewGuid(), SubmissionResultStatus.Accepted));
    }

    [Test]
    public void UpdateResult_UpdatesMatchingResult()
    {
        var submission = CreateSubmission();

        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted, runtime: 100, memoryUsed: 32);

        var result = submission.Results.First(r => r.TestCaseId == TestCaseId1);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Status, Is.EqualTo(SubmissionResultStatus.Accepted));
            Assert.That(result.Runtime, Is.EqualTo(100));
            Assert.That(result.MemoryUsed, Is.EqualTo(32));
        }
    }

    [Test]
    public void UpdateResult_Overwrite_UpdatesExistingResult()
    {
        var submission = CreateSubmission();
        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Processing);

        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted, runtime: 50);

        var result = submission.Results.First(r => r.TestCaseId == TestCaseId1);
        Assert.That(result.Status, Is.EqualTo(SubmissionResultStatus.Accepted));
    }

    [Test]
    public void UpdateResult_DoesNotAffectOtherResults()
    {
        var submission = CreateSubmission();

        submission.UpdateResult(TestCaseId1, SubmissionResultStatus.Accepted);

        var other = submission.Results.First(r => r.TestCaseId == TestCaseId2);
        Assert.That(other.Status, Is.EqualTo(SubmissionResultStatus.Pending));
    }
}
