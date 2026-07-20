using Algowars.Application.Pagination;
using Algowars.Application.Submissions.Dtos;

namespace Algowars.Application.Queries.Problems.GetProblemSubmissions;

public sealed record GetProblemSubmissionsQuery(
    Guid ProblemId,
    PaginationRequest PaginationRequest,
    Guid? UserId,
    bool IncludeAllSubmissions) : IQuery<ProblemSubmissionsPageResult>;
