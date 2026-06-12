using Algowars.Domain.Users;
using ApplicationCore.Commands;
using Ardalis.Result;
using FluentValidation;

namespace Algowars.Application.Commands.User.CreateUser;

internal sealed partial class CreateUserHandler(IValidator<CreateUserCommand> validator, IUserRepository userRepository) : AbstractCommandHandler<CreateUserCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var foundUserBySub = await userRepository.FindBySubAsync(request.Sub, cancellationToken);


    }
}
