using Algowars.Domain.Problems;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Domain.SeedWork;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Entities.Problems;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class ProblemRepository(AlgoWarsDbContext db) : IProblemRepository
{
    public async Task AddAsync(Problem problem)
    {
        var model = MapToDataModel(problem);
        db.Problems.Add(model);
        await db.SaveChangesAsync();
    }

    public async Task<Problem?> FindByIdAsync(Guid id)
    {
        var model = await db.Problems
            .Include(p => p.Versions).ThenInclude(v => v.TestCases)
            .Include(p => p.Versions).ThenInclude(v => v.CodeTemplates)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        return model is null ? null : MapToDomain(model);
    }

    public async Task<Problem?> FindBySlugAsync(Slug slug)
    {
        var model = await db.Problems
            .Include(p => p.Versions).ThenInclude(v => v.TestCases)
            .Include(p => p.Versions).ThenInclude(v => v.CodeTemplates)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug.Value);
        return model is null ? null : MapToDomain(model);
    }

    public async Task UpdateAsync(Problem problem)
    {
        var model = MapToDataModel(problem);
        db.Problems.Update(model);
        await db.SaveChangesAsync();
    }


    public async Task<PageResult<Problem>> GetPageAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var total = await db.Problems.CountAsync(cancellationToken);
        var models = await db.Problems
            .Include(p => p.Versions).ThenInclude(v => v.TestCases)
            .AsNoTracking()
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PageResult<Problem>
        {
            Items = models.Select(MapToDomain).ToList(),
            Total = total,
            Page = page,
            Size = size,
        };
    }

    private static Problem MapToDomain(ProblemDataModel model)
    {
        var slug = new Slug(model.Slug);
        var firstVersion = model.Versions.OrderBy(v => v.VersionNumber).First();
        var problem = new Problem(
            slug,
            new Title(firstVersion.Title),
            new Question(firstVersion.Question),
            new Difficulty(firstVersion.Difficulty),
            new TimeLimit(firstVersion.TimeLimitMs),
            new MemoryLimit(firstVersion.MemoryLimitKb / 1024));
        return problem;
    }

    private static ProblemDataModel MapToDataModel(Problem problem)
    {
        return new ProblemDataModel
        {
            Id = problem.Id,
            Slug = problem.Slug.Value,
            Status = (int)problem.Status,
        };
    }
}
