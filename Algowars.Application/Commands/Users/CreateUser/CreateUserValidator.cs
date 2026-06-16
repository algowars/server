using Algowars.Domain.Users.ValueObjects;
using FluentValidation;

namespace Algowars.Application.Commands.Users.CreateUser;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Sub).NotEmpty();
        RuleFor(x => x.ImageUrl).MaximumLength(ImageUrl.MaxLength);
    }
}
