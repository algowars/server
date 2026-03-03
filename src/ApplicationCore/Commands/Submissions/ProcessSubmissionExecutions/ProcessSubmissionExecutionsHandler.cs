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
        try
        {
            await submissionRepository.ProcessSubmissionInitializationAsync(
                request.submissions,
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
