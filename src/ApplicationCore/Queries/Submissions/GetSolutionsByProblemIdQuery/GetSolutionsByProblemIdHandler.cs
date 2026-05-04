using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSolutionsByProblemIdQuery;

public sealed class GetSolutionsByProblemIdHandler(ISubmissionRepository submissionRepository) : IQueryHandler<GetSolutionsByProblemIdQuery, PaginatedResult<ProblemSubmissionDto>>
{
    public async Task<Result<PaginatedResult<ProblemSubmissionDto>>> Handle(GetSolutionsByProblemIdQuery request, CancellationToken cancellationToken)
    {
        var pageResult = await submissionRepository.GetSubmissionsByProblemId(problemId: request.ProblemId, accountId: null, pagination: request.Pagination, statusFilter: SubmissionStatus.Accepted, cancellationToken: cancellationToken);

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