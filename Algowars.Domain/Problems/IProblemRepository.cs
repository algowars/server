using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;

namespace Algowars.Domain.Problems;

public interface IProblemRepository
{
    Task AddAsync(Entities.Problem problem);
    Task<Entities.Problem?> FindByIdAsync(Guid id);
    Task<Entities.Problem?> FindBySlugAsync(Slug slug);
    Task UpdateAsync(Entities.Problem problem);
}
