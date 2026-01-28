using ApplicationCore.Domain.Accounts;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;

namespace ApplicationCore.Domain.Submissions;

public sealed class SubmissionModel
{
    public required Guid Id { get; init; }

    public string? Code { get; set; }

    public int ProblemSetupId { get; init; }

    public ProblemSetupModel? ProblemSetup { get; init; }

    public DateTime CreatedOn { get; init; }

    public DateTime? CompletedAt { get; set; }

    public Guid CreatedById { get; init; }

    public AccountModel? CreatedBy { get; init; }

    public IEnumerable<SubmissionResult> Results { get; init; } = [];

    public IEnumerable<Guid> GetResultTokens()
    {
        return Results.Select(result => result.Id);
    }

    public SubmissionStatus GetOverallStatus()
    {
        if (
            !Results.Any()
            || Results.Any(r => r.Status is SubmissionStatus.InQueue or SubmissionStatus.Processing)
        )
        {
            return SubmissionStatus.Processing;
        }

        return Results.All(r => r.Status == SubmissionStatus.Accepted)
            ? SubmissionStatus.Accepted
            : SubmissionStatus.WrongAnswer;
    }
}
