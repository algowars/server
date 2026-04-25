using FluentValidation;

namespace ApplicationCore.Commands.Submissions.FinalizeEvaluation;

public sealed class FinalizeEvaluationValidator : AbstractValidator<FinalizeEvaluationCommand>
{
    public FinalizeEvaluationValidator()
    {
        RuleFor(x => x.OutboxIds).NotNull();
    }
}
