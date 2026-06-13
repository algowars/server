using Algowars.Domain.SeedWork;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Factories;
using Algowars.Domain.Users.ValueObjects;
using ApplicationCore.Commands;
using Ardalis.Result;
using FluentValidation;
using UserEntity = Algowars.Domain.Users.Entities.User;

namespace Algowars.Application.Commands.Users.CreateUser;

internal sealed partial class CreateUserHandler(
    IValidator<CreateUserCommand> validator,
    IUserRepository userRepository,
    IAggregateFactory<UserEntity, CreateUserParams> userFactory)
    : AbstractCommandHandler<CreateUserCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = userFactory.Create(new CreateUserParams(request.Username, request.Sub, request.ImageUrl));

        var foundUserBySub = await userRepository.FindBySubAsync(user.Sub, cancellationToken);
        if (foundUserBySub is not null)
            return Result.Conflict("A user with this account already exists.");

        var foundUserByUsername = await userRepository.FindByUsername(user.Username, cancellationToken);
        if (foundUserByUsername is not null)
            return Result.Conflict("Username is already taken.");

        

        await userRepository.AddAsync(user, cancellationToken);

        return Result.Success(user.Id);
    }
}
