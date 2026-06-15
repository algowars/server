using Algowars.Domain.SeedWork;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.Factories;
using Ardalis.Result;
using FluentValidation;

namespace Algowars.Application.Commands.Users.CreateUser;

internal sealed class CreateUserHandler(
    IValidator<CreateUserCommand> validator,
    IUserRepository userRepository,
    IAggregateFactory<User, CreateUserParams> userFactory)
    : AbstractCommandHandler<CreateUserCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = userFactory.Create(new CreateUserParams(request.Username, request.Sub, request.ImageUrl));

        if (await userRepository.FindBySubAsync(user.Sub, cancellationToken) is not null)
            return Result.Conflict("A user with this account already exists.");

        if (await userRepository.FindByUsername(user.Username, cancellationToken) is not null)
            return Result.Conflict("Username is already taken.");

        await userRepository.AddAsync(user, cancellationToken);
        return Result.Success(user.Id);
    }
}
