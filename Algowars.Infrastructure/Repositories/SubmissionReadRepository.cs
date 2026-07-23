using Algowars.Application.Pagination;
using Algowars.Application.Submissions;
using Algowars.Application.Submissions.Dtos;
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
            .Where(s => setupIds.Contains(s.ProblemSetupId));

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        if (!includeAllSubmissions)
        {
            query = query.Where(s => s.Status == SubmissionStatus.Accepted);
        }


        int total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.Id)
            .Skip(offset)
            .Take(paginationRequest.Size)
            .Join(
                context.Users.AsNoTracking(),
                submission => submission.UserId,
                user => user.Id,
                (submission, user) => new ProblemSubmissionDto(
                    submission.Id,
                    submission.Type,
                    submission.Status,
                    DateTime.UtcNow,
                    new SubmissionUserDto(
                        user.Id,
                        user.Username.Value,
                        user.ImageUrl != null ? user.ImageUrl.Value : null)))
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
