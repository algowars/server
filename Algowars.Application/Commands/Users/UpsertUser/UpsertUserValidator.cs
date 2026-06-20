using Algowars.Domain.Users.ValueObjects;
using FluentValidation;

namespace Algowars.Application.Commands.Users.UpsertUser;

internal sealed class UpsertUserValidator : AbstractValidator<UpsertUserCommand>
{
    public UpsertUserValidator()
    {
        RuleFor(x => x.ImageUrl)
            .MaximumLength(ImageUrl.MaxLength);
        RuleFor(x => x.Bio)
            .MaximumLength(Bio.MaxLength);
    }
}
