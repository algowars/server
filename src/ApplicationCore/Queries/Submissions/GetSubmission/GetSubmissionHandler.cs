using ApplicationCore.Dtos.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmission;

public sealed class GetSubmissionHandler(ISubmissionRepository submissionRepository)
    : IQueryHandler<GetSubmissionQuery, SubmissionDto>
{
    public async Task<Result<SubmissionDto>> Handle(
        GetSubmissionQuery request,
        CancellationToken cancellationToken
    )
    {
        var submission = await submissionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (submission is null)
        {
            return Result.NotFound("Submission not found");
        }

        return new SubmissionDto(
            submission.Id,
            submission.OverallStatus,
            submission.Results.Select(result => new SubmissionResultDto(
                result.Status,
                "",
                result.Stdout,
                ""
            ))
        );
    }
}
