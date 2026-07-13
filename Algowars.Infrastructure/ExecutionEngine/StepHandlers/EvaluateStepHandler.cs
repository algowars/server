using System.Text.Json;
using Algowars.Application.ExecutionEngine;
using Algowars.Domain.ExecutionPipelines.Enums;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Enums;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Infrastructure.ExecutionEngine.Assert;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.ExecutionEngine.StepHandlers;

/// <summary>
/// Reads poll results from the previous step, compares actual stdout against expected outputs,
/// updates each <see cref="SubmissionResult"/>, and calls <see cref="Submission.Complete"/>.
/// </summary>
internal sealed partial class EvaluateStepHandler(
    AlgowarsDbContext db,
    ILogger<EvaluateStepHandler> logger) : IStepHandler
{
    public bool CanHandle(ExecutionPipelineStepType stepType)
        => stepType == ExecutionPipelineStepType.Evaluate;

    public async Task<StepHandlerResult> ExecuteAsync(StepHandlerContext context)
    {
        var job = context.Job;
        var attempt = context.Attempt;
        var ct = context.CancellationToken;

        // Load poll results from the most-recent successful poll attempt
        string? pollResponse = job.Attempts
            .Where(a => a.PipelineStepId != attempt.PipelineStepId)
            .OrderByDescending(a => a.StartedAt)
            .Select(a => a.ResponsePayload)
            .FirstOrDefault(p => p is not null);

        if (pollResponse is null)
            return Fail("No poll response found from poll step.");

        List<PollResultEntry>? pollResults;
        try
        {
            pollResults = JsonSerializer.Deserialize<List<PollResultEntry>>(pollResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return Fail("Could not deserialize poll results.");
        }

        if (pollResults is null || pollResults.Count == 0)
            return Fail("Poll results are empty.");

        // Load submission
        var submission = await db.Submissions
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == job.SubmissionId, ct);

        if (submission is null)
            return Fail($"Submission {job.SubmissionId} not found.");

        // Load assert config for this step (optional — defaults to ExactMatch)
        var assertConfig = await db.AssertStepConfigurations
            .FirstOrDefaultAsync(c => c.PipelineStepId == context.Step.Id, ct)
            ?? new AssertStepConfiguration { Strategy = AssertStrategy.ExactMatch, CaseSensitive = true };

        // Load expected outputs for all test cases in one query
        var testCaseIds = pollResults.Select(r => r.TestCaseId).ToList();
        var testCases = await db.Set<TestCase>()
            .Include(tc => tc.ExpectedOutputs)
            .Where(tc => testCaseIds.Contains(tc.Id))
            .ToDictionaryAsync(tc => tc.Id, ct);

        foreach (var pollResult in pollResults)
        {
            if (pollResult.TestCaseId == Guid.Empty)
                continue;

            SubmissionResultStatus status = MapEngineStatus(pollResult.Status);

            if (status == SubmissionResultStatus.Accepted &&
                testCases.TryGetValue(pollResult.TestCaseId, out TestCase? testCase))
            {
                string? expected = testCase.ExpectedOutputs
                    .OrderBy(e => e.Id)
                    .Select(e => e.Value)
                    .FirstOrDefault();

                string? actual = pollResult.Stdout?.TrimEnd('\n', '\r', ' ');

                status = Evaluate(actual, expected, assertConfig);
            }

            submission.UpdateResult(
                testCaseId: pollResult.TestCaseId,
                status: status,
                runtime: pollResult.RuntimeMs,
                memoryUsed: pollResult.MemoryUsedKb,
                actualOutput: pollResult.Stdout,
                standardOutput: pollResult.Stdout,
                standardError: pollResult.Stderr,
                compileOutput: pollResult.CompileOutput);
        }

        // EF cannot detect mutations on private-set properties via snapshot tracking.
        // Explicitly mark every result and the submission itself as Modified.
        foreach (var result in submission.Results)
            db.Entry(result).State = EntityState.Modified;

        db.Entry(submission).State = EntityState.Modified;

        try
        {
            submission.Complete();
        }
        catch (Exception ex)
        {
            return Fail($"Could not complete submission: {ex.Message}");
        }

        await db.SaveChangesAsync(ct);

        LogEvaluated(job.SubmissionId, submission.Status.ToString());

        return new StepHandlerResult(
            Succeeded: true,
            ResponsePayload: JsonSerializer.Serialize(new { submission.Status }));
    }

    private static SubmissionResultStatus Evaluate(
        string? actual,
        string? expected,
        AssertStepConfiguration config)
    {
        if (actual is null || expected is null)
            return SubmissionResultStatus.WrongAnswer;

        bool match = config.Strategy switch
        {
            AssertStrategy.FloatTolerance => TryFloatCompare(actual, expected, config.Tolerance ?? 1e-6m),
            AssertStrategy.SetEquality => NormalizeJson(actual) == NormalizeJson(expected),
            _ => config.CaseSensitive
                ? actual.Trim() == expected.Trim()
                : string.Equals(actual.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase)
        };

        return match ? SubmissionResultStatus.Accepted : SubmissionResultStatus.WrongAnswer;
    }

    private static bool TryFloatCompare(string actual, string expected, decimal tolerance)
    {
        if (decimal.TryParse(actual.Trim(), out decimal a) && decimal.TryParse(expected.Trim(), out decimal e))
            return Math.Abs(a - e) <= tolerance;
        return false;
    }

    private static string NormalizeJson(string value)
    {
        try
        {
            var doc = JsonDocument.Parse(value.Trim());
            return JsonSerializer.Serialize(doc);
        }
        catch
        {
            return value.Trim();
        }
    }

    private static SubmissionResultStatus MapEngineStatus(string? status) =>
        status switch
        {
            "Accepted" => SubmissionResultStatus.Accepted,
            "WrongAnswer" => SubmissionResultStatus.WrongAnswer,
            "TimeLimitExceeded" => SubmissionResultStatus.TimeLimitExceeded,
            "CompilationError" => SubmissionResultStatus.CompileError,
            "RuntimeError" => SubmissionResultStatus.RuntimeError,
            _ => SubmissionResultStatus.RuntimeError
        };

    private static StepHandlerResult Fail(string error) => new(Succeeded: false, Error: error);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Evaluated submission {SubmissionId}: {Status}")]
    private partial void LogEvaluated(Guid submissionId, string status);

    // ── private DTO for deserialising poll step output ────────────────────────

    private sealed record PollResultEntry(
        Guid TestCaseId,
        string? Stdout,
        string? Stderr,
        string? CompileOutput,
        int? RuntimeMs,
        int? MemoryUsedKb,
        string? Status,
        string? Token);
}
