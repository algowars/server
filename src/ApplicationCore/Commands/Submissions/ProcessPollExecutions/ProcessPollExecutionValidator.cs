using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessPollExecutions;

public sealed class ProcessPollExecutionValidator : AbstractValidator<ProcessPollExecutionsCommand>
{
    public ProcessPollExecutionValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}
