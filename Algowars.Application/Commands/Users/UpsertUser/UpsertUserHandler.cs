using Algowars.Domain.Users;
using Algowars.Domain.Users.ValueObjects;
using ApplicationCore.Commands;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Algowars.Application.Commands.Users.UpsertUser;

internal sealed partial class UpsertUserHandler(
    IValidator<UpsertUserCommand> validator,
    IUserWriteRepository userRepository) : AbstractCommandHandler<UpsertUserCommand, Unit>(validator)
{
    protected override async Task<Result<Unit>> HandleValidated(UpsertUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<Unit>.NotFound("User not found.");

        user.UpdateBio(request.Bio is not null ? new Bio(request.Bio) : null);
        user.UpdateImageUrl(request.ImageUrl is not null ? new ImageUrl(request.ImageUrl) : null);

        await userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success(Unit.Value);
    }
}
