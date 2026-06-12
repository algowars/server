using Algowars.Domain.Problem.Entities;
using Algowars.Domain.Problem.ValueObjects;

namespace Algowars.Domain.Problem;

public interface IProblemRepository
{
    Task AddAsync(Entities.Problem problem);
    Task<Entities.Problem?> FindByIdAsync(Guid id);
    Task<Entities.Problem?> FindBySlugAsync(Slug slug);
    Task UpdateAsync(Entities.Problem problem);
}
