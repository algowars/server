using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Queries.Submissions.GetUserSubmissionsByProblemIdQuery;

public sealed class GetUserSubmissionsByProblemIdHandler(ISubmissionRepository submissionRepository) : IQueryHandler<GetUserSubmissionsByProblemIdQuery, PaginatedResult<ProblemSubmissionDto>>
{
    public async Task<Result<PaginatedResult<ProblemSubmissionDto>>> Handle(GetUserSubmissionsByProblemIdQuery request, CancellationToken cancellationToken)
    {
        var pageResult = await submissionRepository.GetUserSolutionsByProblemId(request.ProblemId, request.AccountId, request.Pagination, request.StatusFilter, cancellationToken);

        var dtos = pageResult.Results.Select(submission => new ProblemSubmissionDto(
            CreatedBy: submission?.CreatedBy != null ? new AccountDto
            {
                Username = submission.CreatedBy.Username,
                CreatedOn = submission.CreatedBy.CreatedOn,
                ImageUrl = submission.CreatedBy.ImageUrl
            } : null,
            Code: submission.Code,
            Status: submission.GetOverallStatus().ToString(),
            Language: submission.LanguageVersion.ProgrammingLanguage.Name,
            LanguageVersion: submission.LanguageVersion.Version,
            CreatedOn: submission.CreatedOn,
            RuntimeMs: submission.GetAverageRuntimeMs(),
            MemoryKb: submission.GetAverageMemoryKb()
        )) ?? [];

        return Result.Success(new PaginatedResult<ProblemSubmissionDto>
        {
            Results = [.. dtos],
            Total = pageResult.Total,
            Page = pageResult.Page,
            Size = pageResult.Size,
        });
    }
}
