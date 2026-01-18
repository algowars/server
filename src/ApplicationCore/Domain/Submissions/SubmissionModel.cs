namespace ApplicationCore.Domain.Submissions;

public sealed class SubmissionModel
{
    public required Guid Id { get; init; }

    public required string Code { get; init; }

    public required int ProblemSetupId { get; init; }

    public DateTime CreatedOn { get; init; }

    public DateTime? CompletedAt { get; set; }

    public Guid CreatedById { get; init; }

    public IEnumerable<SubmissionResult> Results { get; init; } = [];

    public IEnumerable<Guid> GetResultTokens()
    {
        return Results.Select(result => result.Id);
    }
}
