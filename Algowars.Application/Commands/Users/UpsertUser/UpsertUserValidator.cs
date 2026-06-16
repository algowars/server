using Algowars.Domain.Users.ValueObjects;
using FluentValidation;

namespace Algowars.Application.Commands.Users.UpsertUser;

internal sealed class UpsertUserValidator : AbstractValidator<UpsertUserCommand>
{
    public UpsertUserValidator()
    {
        RuleFor(x => x.Sub).NotEmpty();

        RuleFor(x => x.ImageUrl)
            .MaximumLength(ImageUrl.MaxLength)
            .When(x => x.ImageUrl is not null);

        RuleFor(x => x.Username)
            .MaximumLength(Username.MaxLength)
            .MinimumLength(Username.MinLength)
            .Must(v => v!.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            .WithMessage("Username contains invalid characters")
            .When(x => x.Username is not null);
    }
}
