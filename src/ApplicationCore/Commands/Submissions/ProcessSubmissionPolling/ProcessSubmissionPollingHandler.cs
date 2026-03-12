using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionPolling;

public sealed class ProcessSubmissionPollingHandler(
    ISubmissionRepository submissionRepository,
    IValidator<ProcessSubmissionPollingCommand> validator
) : AbstractCommandHandler<ProcessSubmissionPollingCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        ProcessSubmissionPollingCommand request,
        CancellationToken cancellationToken
    )
    {
        await submissionRepository.ProcessSubmissionPollingAsync(
            request.Submissions,
            cancellationToken
        );

        return Result.Success();
    }
}
