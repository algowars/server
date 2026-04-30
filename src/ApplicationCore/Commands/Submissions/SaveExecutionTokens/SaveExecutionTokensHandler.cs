using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.SaveExecutionTokens;

public sealed class SaveExecutionTokensHandler(
    ISubmissionRepository submissionRepository,
    IValidator<SaveExecutionTokensCommand> validator
) : AbstractCommandHandler<SaveExecutionTokensCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        SaveExecutionTokensCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await submissionRepository.SaveExecutionTokensAsync(
                request.Submissions,
                cancellationToken
            );

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result<Unit>.Error(ex.Message);
        }
    }
}