using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;

public sealed class ProcessSubmissionExecutionsHandler(
    ISubmissionRepository submissionRepository,
    IValidator<ProcessSubmissionExecutionsCommand> validator
) : AbstractCommandHandler<ProcessSubmissionExecutionsCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        ProcessSubmissionExecutionsCommand request,
        CancellationToken cancellationToken
    )
    {
        await submissionRepository.ProcessSubmissionInitializationAsync(
            request.Submissions,
            cancellationToken
        );

        return Result.Success();
    }
}
