using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public sealed record Judge0SubmissionResponse
{
    [JsonPropertyName("token")]
    public Guid Token { get; init; }

    [JsonPropertyName("source_code")]
    public string? SourceCode { get; set; }

    [JsonPropertyName("language_id")]
    public int LanguageId { get; init; }

    [JsonPropertyName("stdin")]
    public string? Stdin { get; set; }

    [JsonPropertyName("expected_output")]
    public string? ExpectedOutput { get; set; }

    [JsonPropertyName("stdout")]
    public string? Stdout { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("memory")]
    public int? Memory { get; init; }

    [JsonPropertyName("stderr")]
    public string? Stderr { get; set; }

    [JsonPropertyName("status")]
    public required Judge0StatusModel Status { get; init; }
}
