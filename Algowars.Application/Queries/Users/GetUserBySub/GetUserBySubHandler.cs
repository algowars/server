using Algowars.Application.Dtos.Users;
using Algowars.Domain.Users;
using Ardalis.Result;

namespace Algowars.Application.Queries.Users.GetUserBySub;

internal sealed class GetUserBySubHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserBySubQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserBySubQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Sub))
            return Result.Invalid([new ValidationError(nameof(request.Sub), "Sub is required")]);

        var user = await userRepository.FindBySubAsync(request.Sub, cancellationToken);
        if (user is null)
            return Result.NotFound();

        return Result.Success(new UserDto
        {
            Id = user.Id,
            Username = user.Username.Value,
            ImageUrl = user.ImageUrl?.Value,
            Bio = user.Bio?.Value,
            CreatedOn = user.CreatedOn,
            UsernameLastChangedAt = user.UsernameLastChangedAt,
        });
    }
}
