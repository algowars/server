namespace Algowars.Application.Dtos.Submissions;

public sealed record SubmissionDto
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public required string SourceCode { get; init; }
    public required DateTime CreatedOn { get; init; }
}
