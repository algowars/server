using ApplicationCore.Dtos.Submissions;

namespace ApplicationCore.Domain.Submissions;

public sealed class SubmissionModel
{
    public required Guid Id { get; init; }

    public required string Code { get; init; }

    public required int ProblemSetupId { get; init; }

    public DateTime CreatedOn { get; init; }

    public DateTime? CompletedAt { get; set; }

    public Guid CreatedById { get; init; }

    public List<SubmissionResult> Results { get; init; } = [];

    public OverallSubmissionStatus OverallStatus =>
        Results.All(r => r.Status == SubmissionStatus.Accepted)
            ? OverallSubmissionStatus.Accepted
            : OverallSubmissionStatus.WrongAnswer;
}
