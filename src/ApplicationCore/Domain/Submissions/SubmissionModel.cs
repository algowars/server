using ApplicationCore.Domain.Accounts;
using ApplicationCore.Domain.Problems.Languages;

namespace ApplicationCore.Domain.Submissions;

public sealed class SubmissionModel
{
    public required Guid Id { get; init; }

    public string? Code { get; set; }

    public int ProblemSetupId { get; init; }

    public LanguageVersion? LanguageVersion { get; init; }

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
            || Results.Any(r =>
                r.Status == SubmissionStatus.InQueue || r.Status == SubmissionStatus.Processing
            )
        )
        {
            return SubmissionStatus.Processing;
        }

        if (Results.All(r => r.Status == SubmissionStatus.Accepted))
        {
            return SubmissionStatus.Accepted;
        }

        return SubmissionStatus.WrongAnswer;
    }
}
