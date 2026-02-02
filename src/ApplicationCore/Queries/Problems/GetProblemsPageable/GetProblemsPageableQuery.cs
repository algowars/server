using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Problems;

namespace ApplicationCore.Queries.Problems.GetProblemsPageable;

public sealed record GetProblemsPageableQuery(PaginationRequest Pagination)
    : IQuery<PaginatedResult<ProblemDto>>;