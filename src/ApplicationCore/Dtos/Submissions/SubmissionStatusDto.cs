namespace ApplicationCore.Dtos.Submissions;

public sealed class SubmissionStatusDto
{
    public required Guid SubmissionId { get; init; }
    public required string Status { get; init; }
    public required IEnumerable<SubmissionTestCaseResultDto> TestCases { get; init; }
}