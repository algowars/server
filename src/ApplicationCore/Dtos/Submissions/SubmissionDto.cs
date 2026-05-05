using ApplicationCore.Dtos.Accounts;

namespace ApplicationCore.Dtos.Submissions;

public sealed class SubmissionDto
{
    public required Guid Id { get; init; }
    public required int ProblemSetupId { get; init; }
    public required string Status { get; init; }
    public required string Code { get; init; }
    public required DateTime CreatedOn { get; init; }
    public DateTime? CompletedAt { get; init; }
    public AccountDto? CreatedBy { get; init; }
}