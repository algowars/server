using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmissionOutboxes;

public class GetSubmissionOutboxesHandler(ISubmissionRepository submissionRepository)
    : IQueryHandler<GetSubmissionOutboxesQuery, IEnumerable<SubmissionOutboxModel>>
{
    public async Task<Result<IEnumerable<SubmissionOutboxModel>>> Handle(
        GetSubmissionOutboxesQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Result.Success(
                await submissionRepository.GetSubmissionOutboxesAsync(cancellationToken)
            );
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}