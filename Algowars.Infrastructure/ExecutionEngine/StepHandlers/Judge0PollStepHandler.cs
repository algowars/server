using System.Text.Json;
using Algowars.Application.ExecutionEngine;
using Algowars.Domain.ExecutionPipelines.Enums;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.ExecutionEngine.StepHandlers;

/// <summary>
/// Polls Judge0 for the tokens stored by the execute step.
/// Returns <see cref="StepHandlerResult.Succeeded"/> = true only when all tokens have left
/// the queued/processing state. The processor should retry (up to MaxAttempts) if still pending.
/// Stores the polled results JSON in <see cref="StepHandlerResult.ResponsePayload"/>.
/// </summary>
internal sealed partial class Judge0PollStepHandler(
    IExecutionEngineStrategy engine,
    ILogger<Judge0PollStepHandler> logger) : IStepHandler
{
    public bool CanHandle(ExecutionPipelineStepType stepType)
        => stepType == ExecutionPipelineStepType.Judge0Poll;

    public async Task<StepHandlerResult> ExecuteAsync(StepHandlerContext context)
    {
        var job = context.Job;
        var attempt = context.Attempt;
        var ct = context.CancellationToken;

        // The previous execute step stored the token→testCaseId map as its response payload
        var previousResponse = job.Attempts
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

        // Attach the testCaseId back into each result for the evaluate step
        var enriched = results.Select(r => new
        {
            r.Token,
            TestCaseId = tokenMap.TryGetValue(r.Token, out var id) ? id : Guid.Empty,
            r.Stdout,
            r.Stderr,
            r.CompileOutput,
            r.RuntimeMs,
            r.MemoryUsedKb,
            Status = r.Status.ToString()
        }).ToList();

        var responsePayload = JsonSerializer.Serialize(enriched);
        return new StepHandlerResult(Succeeded: true, ResponsePayload: responsePayload);
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Polling {Count} Judge0 tokens for submission {SubmissionId}")]
    private partial void LogPolling(int count, Guid submissionId);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Count} tokens still pending for submission {SubmissionId}")]
    private partial void LogStillPending(int count, Guid submissionId);
}
