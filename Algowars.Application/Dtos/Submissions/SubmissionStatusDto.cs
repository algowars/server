namespace Algowars.Application.Dtos.Submissions;

public sealed record SubmissionStatusDto
{
    public required Guid SubmissionId { get; init; }
    public required string Status { get; init; }
    public required IEnumerable<SubmissionResultDto> Results { get; init; }
}
