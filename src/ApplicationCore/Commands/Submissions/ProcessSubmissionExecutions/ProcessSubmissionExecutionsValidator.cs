using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Commands.Submissions.ProcessSubmissionExecutions;

public sealed class ProcessSubmissionExecutionsValidator
    : AbstractValidator<ProcessSubmissionExecutionsCommand>
{
    public ProcessSubmissionExecutionsValidator()
    {
        RuleFor(x => x.Submissions).NotNull();
    }
}