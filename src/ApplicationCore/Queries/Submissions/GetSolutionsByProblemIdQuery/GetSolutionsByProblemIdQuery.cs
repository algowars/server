using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Dtos.Problems;

namespace ApplicationCore.Queries.Submissions.GetSolutionsByProblemIdQuery;

public sealed record GetSolutionsByProblemIdQuery(
    Guid ProblemId,
    PaginationRequest Pagination
) : IQuery<PaginatedResult<ProblemSubmissionDto>>;
