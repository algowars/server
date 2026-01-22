using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public sealed record Judge0SubmissionResponse
{
    [JsonPropertyName("token")]
    public Guid Token { get; set; }

    [JsonPropertyName("status")]
    public required Judge0StatusModel Status { get; init; }

    [JsonPropertyName("stdout")]
    public string? Stdout { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("memory")]
    public int? MemoryKb { get; init; }
}
