using FluentValidation;

namespace ApplicationCore.Commands.Accounts.UpdateProfileSettings;

public sealed class UpdateProfileSettingsValidator : AbstractValidator<UpdateProfileSettingsCommand>
{
    public UpdateProfileSettingsValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();

        RuleFor(x => x.Bio)
            .MaximumLength(255)
            .WithName("Bio");
    }
}
