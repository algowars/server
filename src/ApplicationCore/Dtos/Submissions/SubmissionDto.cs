using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Dtos.Submissions;

public record SubmissionDto(
    Guid SubmissionId,
    OverallSubmissionStatus Status,
    IEnumerable<SubmissionResultDto> TestCases
);
