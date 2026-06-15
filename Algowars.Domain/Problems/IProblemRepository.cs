using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;

namespace Algowars.Domain.Problems;

public interface IProblemRepository
{
    Task AddAsync(Problem problem);
    Task<Problem?> FindByIdAsync(Guid id);
    Task<Problem?> FindBySlugAsync(Slug slug);
    Task UpdateAsync(Problem problem);
}
