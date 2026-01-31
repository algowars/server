using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmission;

public sealed class GetSubmissionHandler(ISubmissionRepository submissionRepository)
    : IQueryHandler<GetSubmissionQuery, SubmissionModel>
{
    public async Task<Result<SubmissionModel>> Handle(
        GetSubmissionQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await submissionRepository.GetSubmissionAsync(
                request.SubmissionId,
                cancellationToken
            );

            return result is null ? Result.NotFound() : Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
