namespace Algowars.Application.ExecutionEngine;

public sealed record ExecutionEngineSubmission(
    string SourceCode,
    int LanguageId,
    string? Stdin,
    int? TimeLimitMs,
    int? MemoryLimitKb);

public sealed record ExecutionEngineResult(
    string Token,
    string? Stdout,
    string? Stderr,
    string? CompileOutput,
    int? RuntimeMs,
    int? MemoryUsedKb,
    ExecutionEngineResultStatus Status);

public enum ExecutionEngineResultStatus
{
    Queued = 1,
    Processing = 2,
    Accepted = 3,
    WrongAnswer = 4,
    TimeLimitExceeded = 5,
    CompilationError = 6,
    RuntimeError = 7,
    InternalError = 8
}

public interface IExecutionEngineStrategy
{
    /// <summary>Submits a batch of source code runs and returns a token per submission.</summary>
    Task<IReadOnlyList<ExecutionEngineResult>> SubmitBatchAsync(
        IReadOnlyList<ExecutionEngineSubmission> submissions,
        CancellationToken cancellationToken = default);

    /// <summary>Polls for results by token. Returns current status for each token.</summary>
    Task<IReadOnlyList<ExecutionEngineResult>> PollBatchAsync(
        IReadOnlyList<string> tokens,
        CancellationToken cancellationToken = default);
}
