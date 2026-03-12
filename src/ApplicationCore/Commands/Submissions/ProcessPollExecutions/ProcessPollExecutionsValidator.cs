using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessPollExecutions;

public class ProcessPollExecutionsValidator : AbstractValidator<ProcessPollExecutionsCommand>
{
    public ProcessPollExecutionsValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}
