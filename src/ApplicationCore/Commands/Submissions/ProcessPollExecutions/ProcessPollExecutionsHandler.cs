using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessPollExecution;

public sealed class ProcessPollExecutionsHandler(
    ISubmissionRepository submissionRepository,
    IValidator<ProcessPollExecutionsCommand> validator
) : AbstractCommandHandler<ProcessPollExecutionsCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        ProcessPollExecutionsCommand request,
        CancellationToken cancellationToken
    )
    {
        await submissionRepository.ProcessPollExecutionAsync(
            request.Submissions,
            cancellationToken
        );

        return Result.Success();
    }
}