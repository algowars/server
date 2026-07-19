using Algowars.Application.Commands;
using Algowars.Application.Events;
using Algowars.Application.Services.Users;
using Algowars.Domain.SeedWork;
using Algowars.Domain.Users;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.Factories;
using Algowars.Domain.Users.ValueObjects;
using ApplicationCore.Commands;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Algowars.Application.Commands.Users.UpsertUser;

internal sealed partial class UpsertUserHandler(
    IValidator<UpsertUserCommand> validator,
    IAggregateFactory<User, CreateUserParams> userFactory,
    IUsernameGeneratorService usernameGenerator,
    IUserWriteRepository userRepository,
    IDomainEventDispatcher domainEventDispatcher) : AbstractCommandHandler<UpsertUserCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(UpsertUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.FindBySubAsync(request.Sub, cancellationToken);

        if (user is null)
        {
            string username = string.IsNullOrWhiteSpace(request.Username)
                ? usernameGenerator.Generate()
                : request.Username;

            User newUser = userFactory.Create(new CreateUserParams(username, request.Sub, request.ImageUrl));
            newUser.UpdateBio(request.Bio is not null ? new Bio(request.Bio) : null);
            await userRepository.AddAsync(newUser, cancellationToken);
            await domainEventDispatcher.DispatchAsync(newUser.PopDomainEvents(), cancellationToken);
        }
        else
        {
            bool usernameChanged = !string.IsNullOrWhiteSpace(request.Username)
                && request.Username != user.Username.Value;

            if (usernameChanged)
            {
                if (user.UsernameLastChangedAt.HasValue &&
                    DateTime.UtcNow - user.UsernameLastChangedAt.Value < TimeSpan.FromDays(User.MaxDaysUntilUsernameChange))
                    return Result.Invalid(new ValidationError("Username", "Username can only be changed once every 30 days."));

                user.ChangeUsername(new Username(request.Username!));
            }

            user.UpdateBio(request.Bio is not null ? new Bio(request.Bio) : null);
            user.UpdateImageUrl(request.ImageUrl is not null ? new ImageUrl(request.ImageUrl) : null);
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return Result.Success(Unit.Value);
    }
}