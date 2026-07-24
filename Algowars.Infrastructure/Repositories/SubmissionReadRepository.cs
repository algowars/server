using Algowars.Application.Pagination;
using Algowars.Application.Submissions;
using Algowars.Application.Submissions.Dtos;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Submissions.Enums;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class SubmissionReadRepository(AlgowarsDbContext context) : ISubmissionReadRepository
{
    public async Task<PageResult<ProblemSubmissionDto>> GetProblemSubmissionsPagedAsync(
        Guid problemId,
        PaginationRequest paginationRequest,
        Guid? userId,
        bool includeAllSubmissions,
        CancellationToken cancellationToken = default)
    {
        int offset = (paginationRequest.Page - 1) * paginationRequest.Size;

        var setupIds = await context.Problems
            .AsNoTracking()
            .Include(p => p.Setups)
            .Where(p => p.Id == problemId)
            .SelectMany(p => p.Setups)
            .Select(ps => ps.Id)
            .ToListAsync(cancellationToken);

        var query = context.Submissions
            .AsNoTracking()
            .Where(s => setupIds.Contains(s.ProblemSetupId) && s.Type == SubmissionType.Submit);

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        if (!includeAllSubmissions)
        {
            query = query.Where(s => s.Status == SubmissionStatus.Accepted);
        }


        int total = await query.CountAsync(cancellationToken);

        var languageVersions = context.Languages
            .AsNoTracking()
            .SelectMany(
                language => language.Versions,
                (language, version) => new { language, version });

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(offset)
            .Take(paginationRequest.Size)
            .Join(
                context.Users.AsNoTracking(),
                submission => submission.UserId,
                user => user.Id,
                (submission, user) => new { submission, user })
            .Join(
                context.Set<ProblemSetup>().AsNoTracking(),
                x => x.submission.ProblemSetupId,
                setup => setup.Id,
                (x, setup) => new { x.submission, x.user, setup })
            .Join(
                languageVersions,
                x => x.setup.LanguageVersionId,
                lv => lv.version.Id,
                (x, lv) => new ProblemSubmissionDto(
                    x.submission.Id,
                    x.submission.Status,
                    new SubmissionLanguageDto(
                        lv.language.Id,
                        lv.language.Name.Value,
                        lv.version.Version.Value),
                    x.submission.SourceCode.Value,
                    x.submission.CreatedAt,
                    new SubmissionUserDto(
                        x.user.Username.Value,
                        x.user.ImageUrl != null ? x.user.ImageUrl.Value : null),
                    x.submission.MemoryUsage,
                    x.submission.ExecutionTime))
            .ToListAsync(cancellationToken);

        return new PageResult<ProblemSubmissionDto>
        {
            Results = items,
            Total = total,
            Page = paginationRequest.Page,
            Size = paginationRequest.Size,
        };
    }
}