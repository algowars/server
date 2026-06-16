using Algowars.Domain.SeedWork;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.Factories;
using Algowars.Domain.Users.ValueObjects;
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
        if (await userRepository.FindBySubAsync(request.Sub, cancellationToken) is not null)
            return Result.Conflict("A user with this account already exists.");

        string raw = GenerateUsername();
        while (await userRepository.FindByUsername(new Username(raw), cancellationToken) is not null)
            raw = GenerateUsername();

        var user = userFactory.Create(new CreateUserParams(raw, request.Sub, request.ImageUrl));
        await userRepository.AddAsync(user, cancellationToken);
        return Result.Success(user.Id);
    }

    private static string GenerateUsername()
    {
        string suffix = Guid.NewGuid().ToString("N")[..8];
        return $"user_{suffix}";
    }
}
