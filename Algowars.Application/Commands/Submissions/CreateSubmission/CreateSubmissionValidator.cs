
using Algowars.Domain.Submissions.ValueObjects;
using FluentValidation;

namespace Algowars.Application.Commands.Submissions.CreateSubmission;

internal sealed class CreateSubmissionValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionValidator()
    {
        RuleFor(x => x.ProblemSetupId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(SourceCode.MaxLength);
        RuleFor(x => x.CreatedById).NotEmpty();
    }
}