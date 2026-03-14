using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessPollingSubmissionExecutions;

public class ProcessPollingSubmissionExecutionsValidator : AbstractValidator<ProcessPollingSubmissionExecutionsCommand>
{
    public ProcessPollingSubmissionExecutionsValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}