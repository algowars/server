using Algowars.Application.Pagination;
using Algowars.Application.Problems;
using Algowars.Application.Problems.Dtos;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.Enums;
using Algowars.Domain.Problems.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class ProblemReadRepository(AlgowarsDbContext context) : IProblemReadRepository
{
    public async Task<Problem?> FindBySlugAsync(string slug, CancellationToken cancellationToken)
        => await context.Problems
            .AsNoTracking()
            .AsSplitQuery()
            .Include(problem => problem.Setups)
                .ThenInclude(setup => setup.TestSuites)
                    .ThenInclude(testSuite => testSuite.TestCases)
            .Where(problem => problem.Slug.Value == slug)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PageResult<ProblemDto>> GetPagedAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        int offset = (pagination.Page - 1) * pagination.Size;

        var query = context.Problems
            .AsNoTracking()
            .Where(p => p.Status == ProblemStatus.Published);

        int total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip(offset)
            .Take(pagination.Size)
            .Select(p => new
            {
                p.Id,
                Slug = p.Slug.Value,
                Title = p.Title.Value,
                DifficultyValue = p.Difficulty.Value,
                p.Status
            })
            .ToListAsync(cancellationToken);

        return new PageResult<ProblemDto>
        {
            Results = [.. items.Select(x => new ProblemDto(
                x.Id,
                x.Slug,
                x.Title,
                x.DifficultyValue <= Difficulty.EasyMax
                    ? DifficultyTier.Easy
                    : x.DifficultyValue <= Difficulty.MediumMax
                        ? DifficultyTier.Medium
                        : DifficultyTier.Hard,
                x.Status
            ))],
            Total = total,
            Page = pagination.Page,
            Size = pagination.Size
        };
    }
}