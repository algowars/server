using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Dtos.Problems;

namespace ApplicationCore.Queries.Submissions.GetUserSubmissionsByProblemIdQuery;

public sealed record GetUserSubmissionsByProblemIdQuery(
    Guid ProblemId,
    Guid AccountId,
    PaginationRequest Pagination,
    SubmissionStatus? StatusFilter = null
) : IQuery<PaginatedResult<ProblemSubmissionDto>>;