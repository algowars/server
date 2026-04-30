using FluentValidation;

namespace ApplicationCore.Commands.Accounts.UpdateUsername;

public sealed class UpdateUsernameValidator : AbstractValidator<UpdateUsernameCommand>
{
    private const int CooldownDays = 30;

    public UpdateUsernameValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();

        RuleFor(x => x.NewUsername)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(36)
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithName("Username")
            .WithMessage("Username may only contain letters, numbers, underscores, and hyphens.");

        RuleFor(x => x.UsernameLastChangedAt)
            .Must(lastChanged =>
                lastChanged is null ||
                (DateTime.UtcNow - lastChanged.Value).TotalDays >= CooldownDays
            )
            .WithMessage($"Username can only be changed once every {CooldownDays} days.");
    }
}