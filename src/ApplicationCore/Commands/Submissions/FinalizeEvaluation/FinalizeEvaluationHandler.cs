using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.FinalizeEvaluation;

public sealed class FinalizeEvaluationHandler(
    ISubmissionRepository submissionRepository,
    IValidator<FinalizeEvaluationCommand> validator
) : AbstractCommandHandler<FinalizeEvaluationCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        FinalizeEvaluationCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await submissionRepository.FinalizeEvaluationAsync(
                request.OutboxIds,
                request.Now,
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