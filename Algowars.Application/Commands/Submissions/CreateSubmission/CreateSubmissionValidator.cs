using Algowars.Domain.Submissions.ValueObjects;
using FluentValidation;

namespace Algowars.Application.Commands.Submissions.CreateSubmission;

internal sealed class CreateSubmissionValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProblemVersionId).NotEmpty();
        RuleFor(x => x.LanguageVersionId).NotEmpty();
        RuleFor(x => x.SourceCode)
            .NotEmpty()
            .MaximumLength(SourceCode.MaxLength);
        RuleFor(x => x.TestCaseIds).NotEmpty();
    }
}
