using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionPolling;

public sealed class ProcessSubmissionPollingValidator
    : AbstractValidator<ProcessSubmissionPollingCommand>
{
    public ProcessSubmissionPollingValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}
