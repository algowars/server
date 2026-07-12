using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Algowars.Application.Configuration;
using Algowars.Application.ExecutionEngine;

namespace Algowars.Infrastructure.ExecutionEngine.Judge0;

internal sealed class Judge0ExecutionEngineStrategy(
    HttpClient httpClient,
    Judge0Options options) : IExecutionEngineStrategy
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<IReadOnlyList<ExecutionEngineResult>> SubmitBatchAsync(
        IReadOnlyList<ExecutionEngineSubmission> submissions,
        CancellationToken cancellationToken = default)
    {
        var payload = submissions.Select(s => new Judge0SubmissionRequest(
            SourceCode: options.IsEncoded ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s.SourceCode)) : s.SourceCode,
            LanguageId: s.LanguageId,
            Stdin: s.Stdin is null ? null : (options.IsEncoded ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s.Stdin)) : s.Stdin),
            CpuTimeLimit: s.TimeLimitMs.HasValue ? (double?)s.TimeLimitMs.Value / 1000.0 : null,
            MemoryLimit: s.MemoryLimitKb
        )).ToList();

        string url = $"submissions/batch?base64_encoded={options.IsEncoded.ToString().ToLower()}&wait={options.ShouldWait.ToString().ToLower()}";

        using var response = await httpClient.PostAsJsonAsync(url, new { submissions = payload }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokens = await response.Content.ReadFromJsonAsync<List<Judge0TokenResponse>>(JsonOptions, cancellationToken)
            ?? [];

        if (options.ShouldWait)
        {
            return await PollBatchAsync(tokens.Select(t => t.Token).ToList(), cancellationToken);
        }

        return tokens.Select(t => new ExecutionEngineResult(
            Token: t.Token,
            Stdout: null,
            Stderr: null,
            CompileOutput: null,
            RuntimeMs: null,
            MemoryUsedKb: null,
            Status: ExecutionEngineResultStatus.Queued
        )).ToList();
    }

    public async Task<IReadOnlyList<ExecutionEngineResult>> PollBatchAsync(
        IReadOnlyList<string> tokens,
        CancellationToken cancellationToken = default)
    {
        string joined = string.Join(",", tokens);
        string url = $"submissions/batch?tokens={joined}&base64_encoded={options.IsEncoded.ToString().ToLower()}&fields=token,stdout,stderr,compile_output,time,memory,status";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Judge0BatchResponse>(JsonOptions, cancellationToken);

        return result?.Submissions.Select(MapResult).ToList()
            ?? [];
    }

    private ExecutionEngineResult MapResult(Judge0SubmissionResponse r)
    {
        string? Decode(string? value) =>
            value is null ? null
            : options.IsEncoded ? System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value))
            : value;

        var status = r.Status?.Id switch
        {
            1 or 2 => ExecutionEngineResultStatus.Queued,
            3 => ExecutionEngineResultStatus.Accepted,
            4 => ExecutionEngineResultStatus.WrongAnswer,
            5 => ExecutionEngineResultStatus.TimeLimitExceeded,
            6 => ExecutionEngineResultStatus.CompilationError,
            >= 7 and <= 12 => ExecutionEngineResultStatus.RuntimeError,
            _ => ExecutionEngineResultStatus.InternalError
        };

        return new ExecutionEngineResult(
            Token: r.Token ?? string.Empty,
            Stdout: Decode(r.Stdout),
            Stderr: Decode(r.Stderr),
            CompileOutput: Decode(r.CompileOutput),
            RuntimeMs: r.Time.HasValue ? (int)(r.Time.Value * 1000) : null,
            MemoryUsedKb: r.Memory,
            Status: status
        );
    }

    // ── Judge0 request/response shapes ──────────────────────────────────────

    private sealed record Judge0SubmissionRequest(
        [property: JsonPropertyName("source_code")] string SourceCode,
        [property: JsonPropertyName("language_id")] int LanguageId,
        [property: JsonPropertyName("stdin")] string? Stdin,
        [property: JsonPropertyName("cpu_time_limit")] double? CpuTimeLimit,
        [property: JsonPropertyName("memory_limit")] int? MemoryLimit);

    private sealed record Judge0TokenResponse(
        [property: JsonPropertyName("token")] string Token);

    private sealed record Judge0BatchResponse(
        [property: JsonPropertyName("submissions")] List<Judge0SubmissionResponse> Submissions);

    private sealed record Judge0SubmissionResponse(
        [property: JsonPropertyName("token")] string? Token,
        [property: JsonPropertyName("stdout")] string? Stdout,
        [property: JsonPropertyName("stderr")] string? Stderr,
        [property: JsonPropertyName("compile_output")] string? CompileOutput,
        [property: JsonPropertyName("time")] double? Time,
        [property: JsonPropertyName("memory")] int? Memory,
        [property: JsonPropertyName("status")] Judge0StatusResponse? Status);

    private sealed record Judge0StatusResponse(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("description")] string Description);
}
