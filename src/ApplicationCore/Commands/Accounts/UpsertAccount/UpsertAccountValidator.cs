using FluentValidation;

namespace ApplicationCore.Commands.Accounts.UpsertAccount;

public sealed class UpsertAccountValidator : AbstractValidator<UpsertAccountCommand>
{
    public UpsertAccountValidator()
    {
        RuleFor(x => x.Sub)
            .NotEmpty()
            .MaximumLength(255);
    }
}
