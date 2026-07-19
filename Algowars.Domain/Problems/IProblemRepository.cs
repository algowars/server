using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems;

public interface IProblemRepository : IRepository<Problem>
{
    Task<Problem?> FindBySetupIdAsync(Guid setupId, CancellationToken cancellationToken = default);

    Task<Problem?> FindBySlugAsync(Slug slug, CancellationToken cancellationToken = default);
}