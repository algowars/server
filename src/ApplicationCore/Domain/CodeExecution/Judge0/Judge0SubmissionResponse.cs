using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public sealed record Judge0SubmissionResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; init; }

    [JsonPropertyName("status")]
    public Judge0StatusModel? Status { get; init; }

    [JsonPropertyName("stdout")]
    public string? Stdout { get; init; }

    [JsonPropertyName("stderr")]
    public string? Stderr { get; init; }

    [JsonPropertyName("compile_output")]
    public string? CompileOutput { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("time")]
    public float? Time { get; init; }

    [JsonPropertyName("memory")]
    public float? Memory { get; init; }
}
