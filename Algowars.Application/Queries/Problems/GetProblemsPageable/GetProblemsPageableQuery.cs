using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;

namespace Algowars.Application.Queries.Problems.GetProblemsPageable;

public sealed record GetProblemsPageableQuery(PaginationRequest PaginationRequest) : IQuery<PageResult<ProblemDto>>;