using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmissionsPaginated;

public sealed class GetSubmissionsPaginatedHandler(ISubmissionRepository repository)
    : IQueryHandler<GetSubmissionsPaginatedQuery, PaginatedResult<SubmissionDto>>
{
    public async Task<Result<PaginatedResult<SubmissionDto>>> Handle(
        GetSubmissionsPaginatedQuery request,
        CancellationToken cancellationToken
    )
    {
        SubmissionStatus? statusFilter = request.AcceptedOnly ? SubmissionStatus.Accepted : null;
        
        var page = await repository.GetSubmissionsByProblemId(
            request.ProblemId,
            request.FilterByUserId,
            request.Pagination,
            statusFilter,
            cancellationToken
        );

        var dtoItems = page.Results
            .Select(s => new SubmissionDto
            {
                Id = s.Id,
                ProblemSetupId = s.ProblemSetupId,
                Status = s.GetOverallStatus().ToString(),
                Code = s.Code ?? string.Empty,
                CreatedOn = s.CreatedOn,
                CompletedAt = s.CompletedAt,
                CreatedBy = s.CreatedBy is null ? null : new AccountDto
                {
                    Id = s.CreatedBy.Id,
                    Username = s.CreatedBy.Username,
                    ImageUrl = s.CreatedBy.ImageUrl,
                    CreatedOn = s.CreatedBy.CreatedOn,
                },
            })
            .ToList();

        return Result.Success(new PaginatedResult<SubmissionDto>
        {
            Results = dtoItems,
            Total = page.Total,
            Page = page.Page,
            Size = page.Size,
        });
    }
}
