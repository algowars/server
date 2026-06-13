using Algowars.Domain.Users.ValueObjects;
using FluentValidation;

namespace Algowars.Application.Commands.Users.CreateUser;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(Username.MaxLength)
                .MinimumLength(Username.MinLength)
                .Must(IsUsernameValid)
                .WithMessage("Username contains invalid characters");

        RuleFor(x => x.Sub)
            .NotEmpty();

        RuleFor(x => x.ImageUrl)
            .MaximumLength(ImageUrl.MaxLength);
    }

    private static bool IsUsernameValid(string username) => 
        username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
}
