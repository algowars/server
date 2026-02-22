using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Problems.Admin;

namespace ApplicationCore.Queries.Problems.GetAdminProblemsPageable;

public sealed record GetAdminProblemsPageableQuery(PaginationRequest Pagination)
    : IQuery<PaginatedResult<AdminProblemDto>>;