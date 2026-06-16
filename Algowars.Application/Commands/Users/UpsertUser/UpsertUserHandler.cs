using Algowars.Domain.SeedWork;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.Factories;
using Algowars.Domain.Users.ValueObjects;
using Ardalis.Result;
using FluentValidation;

namespace Algowars.Application.Commands.Users.UpsertUser;

internal sealed class UpsertUserHandler(
    IValidator<UpsertUserCommand> validator,
    IUserRepository userRepository,
    IAggregateFactory<User, CreateUserParams> userFactory)
    : AbstractCommandHandler<UpsertUserCommand, Guid>(validator)
{
    protected override async Task<Result<Guid>> HandleValidated(UpsertUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.FindBySubAsync(request.Sub, cancellationToken);

        if (existing is not null)
        {
            return await UpdateExisting(existing, request, cancellationToken);
        }

        return await CreateNew(request, cancellationToken);
    }

    private async Task<Result<Guid>> UpdateExisting(User user, UpsertUserCommand request, CancellationToken cancellationToken)
    {
        user.UpdateImageUrl(request.ImageUrl is not null ? new ImageUrl(request.ImageUrl) : null);

        if (request.Username is not null)
        {
            var desiredUsername = new Username(request.Username);

            if (user.Username.Value != desiredUsername.Value)
            {
                var taken = await userRepository.FindByUsername(desiredUsername, cancellationToken);
                if (taken is not null)
                {
                    return Result.Conflict("Username is already taken.");
                }

                try
                {
                    user.ChangeUsername(desiredUsername);
                }
                catch (Exception ex)
                {
                    return Result.Error(ex.Message);
                }
            }
        }

        await userRepository.UpdateAsync(user, cancellationToken);
        return Result.Success(user.Id);
    }

    private async Task<Result<Guid>> CreateNew(UpsertUserCommand request, CancellationToken cancellationToken)
    {
        string raw = request.Username ?? await GenerateUniqueUsername(cancellationToken);

        if (request.Username is not null)
        {
            var taken = await userRepository.FindByUsername(new Username(raw), cancellationToken);
            if (taken is not null)
            {
                return Result.Conflict("Username is already taken.");
            }
        }

        var user = userFactory.Create(new CreateUserParams(raw, request.Sub, request.ImageUrl));
        await userRepository.AddAsync(user, cancellationToken);
        return Result.Success(user.Id);
    }

    private async Task<string> GenerateUniqueUsername(CancellationToken cancellationToken)
    {
        string raw;
        do
        {
            raw = $"user_{Guid.NewGuid():N}"[..13];
        }
        while (await userRepository.FindByUsername(new Username(raw), cancellationToken) is not null);

        return raw;
    }
}
