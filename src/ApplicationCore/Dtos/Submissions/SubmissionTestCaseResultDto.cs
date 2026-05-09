namespace ApplicationCore.Dtos.Submissions;

public sealed class SubmissionTestCaseResultDto
{
    public string Input { get; init; } = string.Empty;
    public string ExpectedOutput { get; init; } = string.Empty;
    public string ActualOutput { get; init; } = string.Empty;
    public int Status { get; init; }
}
