using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Queries.Submissions.GetProblemSubmissions;

public sealed record GetProblemSubmissionsQuery() : IQuery<ProblemSubmissions>;
