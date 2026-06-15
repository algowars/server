using Algowars.Application.Common.Pagination;
using Algowars.Application.Dtos.Problems;

namespace Algowars.Application.Queries.Problems.GetProblemsPageable;

public sealed record GetProblemsPageableQuery(PaginationRequest Pagination) : IQuery<PaginatedResult<ProblemDto>>;
