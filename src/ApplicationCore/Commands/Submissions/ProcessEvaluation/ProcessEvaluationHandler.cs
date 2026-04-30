using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace ApplicationCore.Commands.Submissions.ProcessEvaluation;

public sealed class ProcessEvaluationHandler(
    ISubmissionRepository submissionRepository,
    IValidator<ProcessEvaluationCommand> validator
) : AbstractCommandHandler<ProcessEvaluationCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(
        ProcessEvaluationCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await submissionRepository.ProcessEvaluationAsync(
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