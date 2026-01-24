using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetSubmissionExecutionOutboxes;

public sealed class GetSubmissionExecutionOutboxesHandler(
    ISubmissionRepository submissionRepository
) : IQueryHandler<GetSubmissionExecutionOutboxesCommand, IEnumerable<SubmissionOutboxModel>>
{
    public async Task<Result<IEnumerable<SubmissionOutboxModel>>> Handle(
        GetSubmissionExecutionOutboxesCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Result.Success(
                await submissionRepository.GetSubmissionExecutionOutboxesAsync(cancellationToken)
            );
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
