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
        await submissionRepository.ProcessEvaluationAsync(request.Contexts, cancellationToken);

        return Result.Success();
    }
}
