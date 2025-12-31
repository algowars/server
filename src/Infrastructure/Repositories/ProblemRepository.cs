using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Interfaces.Repositories;
using Infrastructure.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ProblemRepository(IAppDbContext db) : IProblemRepository
{
    private readonly IAppDbContext _db = db;
    private static readonly int AcceptedProblemStatusId = 1;

    public async Task<ProblemModel?> GetProblemByIdAsync(
        Guid problemId,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .Problems.ProjectToType<ProblemModel>()
            .SingleOrDefaultAsync(p => p.Id == problemId, cancellationToken);
    }

    public async Task<ProblemModel?> GetProblemBySlugAsync(
        string slug,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .Problems.ProjectToType<ProblemModel>()
            .SingleOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<PaginatedResult<ProblemModel>> GetProblemsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken
    )
    {
        int page = pagination.Page > 0 ? pagination.Page : 1;
        int size = pagination.Size > 0 ? pagination.Size : 10;

        var baseQuery = _db
            .Problems.Include(p => p.Tags)
            .Include(p => p.Status)
            .Where(p =>
                p.CreatedOn <= pagination.Timestamp
                && p.DeletedOn == null
                && p.StatusId == AcceptedProblemStatusId
            );

        var ordered =
            pagination.Direction == SortDirection.Asc
                ? baseQuery.OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                : baseQuery.OrderByDescending(p => p.CreatedOn).ThenByDescending(p => p.Id);

        int total = await ordered.CountAsync(cancellationToken);

        var problems = await ordered
            .Skip((page - 1) * size)
            .Take(size)
            .ProjectToType<ProblemModel>()
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ProblemModel>
        {
            Results = problems,
            Total = total,
            Page = page,
            Size = size,
        };
    }

    public async Task<ProblemSetupModel> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .ProblemSetups.Include(ps => ps.LanguageVersion)
            .Include(ps => ps.TestSuites)
                .ThenInclude(ts => ts.TestCases)
            .Where(ps =>
                ps.ProblemId == problemId && ps.ProgrammingLanguageVersionId == languageVersionId
            )
            .ProjectToType<ProblemSetupModel>()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProblemSetupModel?> GetProblemSetupAsync(
        int setupId,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .ProblemSetups.Include(ps => ps.LanguageVersion)
            .Include(ps => ps.TestSuites)
                .ThenInclude(ts => ts.TestCases)
            .Where(ps => ps.Id == setupId)
            .ProjectToType<ProblemSetupModel>()
            .SingleOrDefaultAsync(cancellationToken);
    }
}
