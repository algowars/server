using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems;

public interface IProblemRepository : IRepository<Entities.Problem>
{
    Task<Entities.Problem?> FindBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
}
