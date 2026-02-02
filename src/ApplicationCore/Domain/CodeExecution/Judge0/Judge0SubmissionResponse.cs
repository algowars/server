using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public sealed record Judge0SubmissionResponse
{
    [JsonPropertyName("token")]
    public Guid Token { get; init; }

    [JsonPropertyName("source_code")]
    public string? SourceCode { get; init; }

    [JsonPropertyName("language_id")]
    public int LanguageId { get; init; }

    [JsonPropertyName("stdin")]
    public string? Stdin { get; init; }

    [JsonPropertyName("expected_output")]
    public string? ExpectedOutput { get; init; }

    [JsonPropertyName("stdout")]
    public string? Stdout { get; init; }

    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("memory")]
    public int? Memory { get; init; }

    [JsonPropertyName("stderr")]
    public string? Stderr { get; init; }

    [JsonPropertyName("status")]
    public required Judge0StatusModel Status { get; init; }
}