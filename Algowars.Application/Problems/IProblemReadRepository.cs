using Algowars.Application.Pagination;
using Algowars.Application.Problems.Dtos;
using Algowars.Domain.Problems.Entities;

namespace Algowars.Application.Problems;

public interface IProblemReadRepository
{
    Task<PageResult<ProblemDto>> GetPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);

    Task<Problem?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
