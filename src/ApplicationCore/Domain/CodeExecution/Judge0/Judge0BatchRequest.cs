using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public sealed class Judge0BatchRequest
{
    [JsonPropertyName("submissions")]
    public required IEnumerable<Judge0SubmissionRequest> Submissions { get; init; }
}