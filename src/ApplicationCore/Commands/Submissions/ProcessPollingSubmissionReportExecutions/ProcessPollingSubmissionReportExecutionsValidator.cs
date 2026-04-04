using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessPollingSubmissionReportExecutions;

public class ProcessPollingSubmissionReportExecutionsValidator : AbstractValidator<ProcessPollingSubmissionReportExecutionsCommand>
{
    public ProcessPollingSubmissionReportExecutionsValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}
