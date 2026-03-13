using ApplicationCore.Commands.Submissions.ProcessPollExecutions;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessPollExecutions;

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
        await submissionRepository.ProcessPollSubmissionsAsync(
            request.Submissions,
            cancellationToken
        );

        return Result.Success();
    }
}
