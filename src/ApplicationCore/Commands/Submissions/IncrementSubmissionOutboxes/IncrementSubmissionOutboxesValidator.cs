using FluentValidation;

namespace ApplicationCore.Commands.Submissions.IncrementSubmissionOutboxes;

public sealed class IncrementSubmissionOutboxesValidator
    : AbstractValidator<IncrementSubmissionOutboxesCommand>
{
    public IncrementSubmissionOutboxesValidator()
    {
        RuleFor(x => x.OutboxIds).NotEmpty();

        RuleFor(x => x.Timestamp).LessThanOrEqualTo(DateTime.UtcNow);
    }
}