using System.Text.Json.Serialization;

namespace ApplicationCore.Domain.CodeExecution.Judge0;

public class Judge0SubmissionTokenOnlyResponse
{
    [JsonPropertyName("token")]
    public Guid Token { get; init; }
}
