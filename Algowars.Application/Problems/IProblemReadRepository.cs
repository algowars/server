using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;

namespace Algowars.Application.Problems;

public interface IProblemReadRepository
{
    Task<PageResult<ProblemDto>> GetPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
}
