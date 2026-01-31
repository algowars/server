using System.Text.Json.Serialization;
using ApplicationCore.Domain.CodeExecution.Judge0;

namespace ApplicationCore.Domain.Submissions;

public sealed record Judge0BatchGetResponse
{
    [JsonPropertyName("submissions")]
    public required List<Judge0SubmissionResponse> Submissions { get; init; }
}
