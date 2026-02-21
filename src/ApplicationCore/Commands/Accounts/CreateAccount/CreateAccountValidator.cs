using ApplicationCore.Interfaces.Repositories;
using FluentValidation;

namespace ApplicationCore.Commands.Accounts.CreateAccount;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator(IAccountRepository accounts)
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(16)
            .Must(IsValidUsername)
            .WithMessage("Username contains invalid characters")
            .MustAsync(
                async (username, ct) => await accounts.GetByUsernameAsync(username, ct) is null
            )
            .WithMessage("Username already exists");

        RuleFor(x => x.Sub)
            .NotEmpty()
            .MustAsync(async (sub, ct) => await accounts.GetBySubAsync(sub, ct) is null)
            .WithMessage("Account already exists");

        RuleFor(x => x.ImageUrl)
            .Must(IsValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("ImageUrl must be a valid URL");

        RuleFor(x => x)
            .MustAsync(
                async (cmd, ct) =>
                    await accounts.GetByUsernameOrSubAsync(cmd.Username, cmd.Sub, ct) is null
            )
            .WithMessage("Username already exists");
    }

    private static bool IsValidUsername(string username) =>
        username.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    private static bool IsValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}