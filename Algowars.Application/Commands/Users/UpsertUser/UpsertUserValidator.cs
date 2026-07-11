using Algowars.Domain.Users.ValueObjects;
using FluentValidation;

namespace Algowars.Application.Commands.Users.UpsertUser;

internal sealed class UpsertUserValidator : AbstractValidator<UpsertUserCommand>
{
    public UpsertUserValidator()
    {
        RuleFor(x => x.Username)
            .MaximumLength(Username.MaxLength)
            .When(x => x.Username is not null);
        RuleFor(x => x.ImageUrl)
            .MaximumLength(ImageUrl.MaxLength);
        RuleFor(x => x.Bio)
            .MaximumLength(Bio.MaxLength);
    }
}