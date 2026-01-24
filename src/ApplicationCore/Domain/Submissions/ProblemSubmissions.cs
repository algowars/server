using ApplicationCore.Domain.Problems;

namespace ApplicationCore.Domain.Submissions;

public sealed class ProblemSubmissions
{
    public required ProblemModel Problem { get; init; }

    public required IEnumerable<SubmissionModel> Submissions { get; init; }
}
