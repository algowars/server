using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;

public sealed class IncrementSubmissionOutboxesHandler(
    ISubmissionRepository submissionRepository,
    IValidator<IncrementSubmissionOutboxesCommand> validator
) : AbstractCommandHandler<IncrementSubmissionOutboxesCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        IncrementSubmissionOutboxesCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await submissionRepository.IncrementOutboxesCountAsync(
                request.OutboxIds,
                request.Timestamp,
                cancellationToken
            );

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
