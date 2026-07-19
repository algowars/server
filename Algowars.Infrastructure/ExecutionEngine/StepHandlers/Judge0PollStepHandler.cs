using Algowars.Application.ExecutionEngine;
using Algowars.Domain.ExecutionPipelines.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Algowars.Infrastructure.ExecutionEngine.StepHandlers;

/// <summary>
/// Polls Judge0 for the tokens stored by the execute step.
/// Returns <see cref="StepHandlerResult.Succeeded"/> = true only when all tokens have left
/// the queued/processing state. The processor should retry (up to MaxAttempts) if still pending.
/// Stores the polled results JSON in <see cref="StepHandlerResult.ResponsePayload"/>.
/// </summary>
internal sealed partial class Judge0PollStepHandler(IExecutionEngineStrategy engine, ILogger<Judge0PollStepHandler> logger) : IStepHandler
{
    public bool CanHandle(ExecutionPipelineStepType stepType) => stepType == ExecutionPipelineStepType.Judge0Poll;

    public async Task<StepHandlerResult> ExecuteAsync(StepHandlerContext context)
    {
        var job = context.Job;
        var attempt = context.Attempt;
        var ct = context.CancellationToken;

        string? previousResponse = job.Attempts
            .Where(a => a.PipelineStepId != attempt.PipelineStepId)
            .OrderByDescending(a => a.StartedAt)
            .Select(a => a.ResponsePayload)
            .FirstOrDefault(p => p is not null);

        if (previousResponse is null)
            return new StepHandlerResult(Succeeded: false, Error: "No token map found from execute step.");

        Dictionary<string, Guid>? tokenMap;
        try
        {
            tokenMap = JsonSerializer.Deserialize<Dictionary<string, Guid>>(previousResponse);
        }
        catch
        {
            return new StepHandlerResult(Succeeded: false, Error: "Could not deserialize token map from execute step.");
        }

        if (tokenMap is null || tokenMap.Count == 0)
            return new StepHandlerResult(Succeeded: false, Error: "Token map is empty.");

        LogPolling(tokenMap.Count, job.SubmissionId);

        IReadOnlyList<ExecutionEngineResult> results;
        try
        {
            results = await engine.PollBatchAsync(tokenMap.Keys.ToList(), ct);
        }
        catch (Exception ex)
        {
            return new StepHandlerResult(Succeeded: false, Error: $"Judge0 poll failed: {ex.Message}");
        }

        bool allDone = results.All(r =>
              r.Status != ExecutionEngineResultStatus.Queued &&
              r.Status != ExecutionEngineResultStatus.Processing);

        if (!allDone)
        {
            LogStillPending(results.Count(r =>
                r.Status is ExecutionEngineResultStatus.Queued or ExecutionEngineResultStatus.Processing),
                job.SubmissionId);
            return new StepHandlerResult(Succeeded: false, Error: "Submissions still processing; will retry.");
        }

        var enriched = results.Select(r =>
        {
            var parsedOutput = ParseOutput(r.Stdout);

            return new
            {
                r.Token,
                TestCaseId = tokenMap.TryGetValue(r.Token, out var id) ? id : Guid.Empty,
                Stdout = parsedOutput.UserLogs,
                ActualOutput = parsedOutput.ActualResult,
                r.Stderr,
                r.CompileOutput,
                r.RuntimeMs,
                r.MemoryUsedKb,
                Status = r.Status.ToString()
            };
        }).ToList();

        string responsePayload = JsonSerializer.Serialize(enriched);
        return new StepHandlerResult(Succeeded: true, ResponsePayload: responsePayload);
    }

    private static ParsedOutput ParseOutput(string? stdout)
    {
        if (string.IsNullOrEmpty(stdout))
            return new ParsedOutput(UserLogs: null, ActualResult: null);

        string[] lines = stdout.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // If only one line, it's the result with no logs
        if (lines.Length == 1)
            return new ParsedOutput(UserLogs: null, ActualResult: lines[0]);

        // Last line is the result, everything before is logs (comma-joined)
        string actualResult = lines[^1];
        string[] logLines = lines[..^1];

        string userLogs = logLines.Length == 0 
            ? null 
            : string.Join(",", logLines);

        return new ParsedOutput(
            UserLogs: userLogs,
            ActualResult: string.IsNullOrWhiteSpace(actualResult) ? null : actualResult);
    }

    private sealed record ParsedOutput(string? UserLogs, string? ActualResult);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Polling {Count} Judge0 tokens for submission {SubmissionId}")]
    private partial void LogPolling(int count, Guid submissionId);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Count} tokens still pending for submission {SubmissionId}")]
    private partial void LogStillPending(int count, Guid submissionId);
}
