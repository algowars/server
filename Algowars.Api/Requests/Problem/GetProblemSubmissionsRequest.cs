namespace Algowars.Api.Requests.Problem;

public sealed record GetProblemSubmissionsRequest(
    Guid ProblemId,
    int Page,
    int Size,
    DateTime Timestamp,
    SubmissionFilterType Filter = SubmissionFilterType.All);
