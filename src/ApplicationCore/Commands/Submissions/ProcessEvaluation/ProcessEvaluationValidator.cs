using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessEvaluation;

public sealed class ProcessEvaluationValidator : AbstractValidator<ProcessEvaluationCommand>
{
    public ProcessEvaluationValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}