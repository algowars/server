using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public sealed record Judge0BatchGetResponse
{
    [JsonPropertyName("submissions")]
    public required List<Judge0SubmissionResponse> Submissions { get; init; }
}