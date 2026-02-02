using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public class Judge0SubmissionRequest
{
    [JsonPropertyName("language_id")]
    public int LanguageId { get; init; }

    [JsonPropertyName("source_code")]
    public string? SourceCode { get; init; }

    [JsonPropertyName("stdin")]
    public string? StdIn { get; init; }

    [JsonPropertyName("expected_output")]
    public string? ExpectedOutput { get; init; }
}