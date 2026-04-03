using FluentValidation;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionReportExecution;

public sealed class ProcessSubmissionReportExecutionValidator
    : AbstractValidator<ProcessSubmissionReportExecutionCommand>
{
    public ProcessSubmissionReportExecutionValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}
