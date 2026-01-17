using FluentValidation;

namespace ApplicationCore.Commands.Submissions.CreateSubmission;

public sealed class CreateSubmissionValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50_000);

        RuleFor(x => x.ProblemSetupId).GreaterThan(0);

        RuleFor(x => x.CreatedById).NotEmpty();
    }
}
