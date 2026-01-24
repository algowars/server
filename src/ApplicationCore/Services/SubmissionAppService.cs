using ApplicationCore.Commands.Submissions.CreateSubmission;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Submissions;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Submissions.GetSubmissions;
using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Services;

public sealed class SubmissionAppService(IMediator mediator) : ISubmissionAppService
{
    public Task<Result<Guid>> CreateAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateSubmissionCommand(problemSetupId, code, createdById);

        return mediator.Send(command, cancellationToken);
    }

    public async Task<Result<PaginatedResult<SubmissionDto>>> GetSubmissionsAsync(
        Guid problemId,
        PaginationRequest paginationRequest,
        CancellationToken cancellationToken
    )
    {
        var query = new GetSubmissionsQuery(problemId, paginationRequest);

        var paginatedResult = await mediator.Send(query, cancellationToken);

        return new PaginatedResult<SubmissionDto>
        {
            Results =
            [
                .. paginatedResult.Value.Results.Select(submission => new SubmissionDto(
                    Id: submission.Id,
                    Code: submission.Code ?? string.Empty,
                    LanguageVersion: new LanguageVersionDto
                    {
                        Id = submission.LanguageVersion!.Id,
                        Version = submission.LanguageVersion.Version,
                        ProgrammingLanguage =
                            submission.LanguageVersion.ProgrammingLanguage != null
                                ? new ProgrammingLanguageDto
                                {
                                    Id = submission.LanguageVersion.ProgrammingLanguage.Id,
                                    Name = submission.LanguageVersion.ProgrammingLanguage.Name,
                                }
                                : null,
                    },
                    CreatedBy: new AccountDto
                    {
                        Id = submission.CreatedBy!.Id,
                        Username = submission.CreatedBy.Username,
                    },
                    CreatedOn: submission.CreatedOn,
                    Status: submission.GetOverallStatus()
                )),
            ],
            Total = paginatedResult.Value.Total,
            Page = paginatedResult.Value.Page,
            Size = paginatedResult.Value.Size,
        };
    }
}
