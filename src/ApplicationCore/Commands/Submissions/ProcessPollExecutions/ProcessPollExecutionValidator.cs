using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessPollExecution;

public sealed class ProcessPollExecutionValidator
    : AbstractValidator<ProcessPollExecutionsCommand>
{
    public ProcessPollExecutionValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}