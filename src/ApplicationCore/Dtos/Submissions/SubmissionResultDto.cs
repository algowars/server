using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Dtos.Submissions;

public record SubmissionResultDto(
    SubmissionStatus Status,
    string Input,
    string Output,
    string ExpectedOutput
);
