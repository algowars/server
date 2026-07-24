using Algowars.Domain.SeedWork;
using Algowars.Domain.Submissions.Enums;
using Algowars.Domain.Submissions.Events;
using Algowars.Domain.Submissions.Exceptions;
using Algowars.Domain.Submissions.ValueObjects;

namespace Algowars.Domain.Submissions.Entities;

public sealed class Submission : AggregateRoot
{
    public Submission(
        Guid userId,
        Guid problemSetupId,
        SubmissionType type,
        SourceCode sourceCode,
        IEnumerable<Guid> testCaseIds)
    {
        UserId = userId;
        ProblemSetupId = problemSetupId;
        Type = type;
        SourceCode = sourceCode ?? throw new ArgumentNullException(nameof(sourceCode));
        Status = SubmissionStatus.Queued;
        CreatedAt = DateTime.UtcNow;

        foreach (Guid testCaseId in testCaseIds)
            _results.Add(new SubmissionResult(testCaseId));

        AddDomainEvent(new SubmissionCreatedDomainEvent(Id));
    }

    public void Complete()
    {
        if (_results.Any(r => !r.IsTerminal))
            throw new SubmissionNotCompleteException();

        Status = _results.All(r => r.Status == SubmissionResultStatus.Accepted)
            ? SubmissionStatus.Accepted
            : SubmissionStatus.WrongAnswer;
    }

    public void StartRunning()
    {
        if (Status != SubmissionStatus.Queued)
            throw new InvalidSubmissionStateException("Only a queued submission can be started.");

        Status = SubmissionStatus.Running;
    }

    public void UpdateResult(
        Guid testCaseId,
        SubmissionResultStatus status,
        int? runtime = null,
        int? memoryUsed = null,
        string? actualOutput = null,
        string? standardOutput = null,
        string? standardError = null,
        string? compileOutput = null)
    {
        SubmissionResult result = _results.FirstOrDefault(r => r.TestCaseId == testCaseId)
            ?? throw new SubmissionResultNotFoundException(testCaseId);

        result.Update(status, runtime, memoryUsed, actualOutput, standardOutput, standardError, compileOutput);
    }

    private Submission() { }

    public DateTime CreatedAt { get; private set; }

    public int? ExecutionTime => Results.Max(r => r.Runtime);

    public int? MemoryUsage => Results.Max(r => r.MemoryUsed);

    public Guid ProblemSetupId { get; private set; }
    public IReadOnlyCollection<SubmissionResult> Results => _results.AsReadOnly();
    public SourceCode SourceCode { get; private set; } = null!;
    public SubmissionStatus Status { get; private set; }
    public SubmissionType Type { get; private set; }
    public Guid UserId { get; private set; }


    private readonly List<SubmissionResult> _results = [];
}