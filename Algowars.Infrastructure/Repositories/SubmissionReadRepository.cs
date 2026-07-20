using Algowars.Application.Pagination;
using Algowars.Application.Submissions;
using Algowars.Application.Submissions.Dtos;
using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class SubmissionReadRepository(AlgowarsDbContext context) : ISubmissionReadRepository
{
    public async Task<ProblemSubmissionsPageResult> GetProblemSubmissionsPagedAsync(
        Guid problemId,
        Guid? userId,
        PaginationRequest paginationRequest,
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

        return new ProblemSubmissionsPageResult(
            items,
            paginationRequest.Page,
            paginationRequest.Size,
            total,
            paginationRequest.Timestamp);
    }
}
