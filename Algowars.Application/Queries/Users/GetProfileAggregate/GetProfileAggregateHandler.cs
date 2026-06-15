using Algowars.Application.Dtos.Users;
using Algowars.Domain.Users;
using Algowars.Domain.Users.ValueObjects;
using Ardalis.Result;

namespace Algowars.Application.Queries.Users.GetProfileAggregate;

internal sealed class GetProfileAggregateHandler(IUserRepository userRepository)
    : IQueryHandler<GetProfileAggregateQuery, ProfileAggregateDto>
{
    public async Task<Result<ProfileAggregateDto>> Handle(GetProfileAggregateQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return Result.Invalid([new ValidationError(nameof(request.Username), "Username is required")]);

        var user = await userRepository.FindByUsername(new Username(request.Username), cancellationToken);
        if (user is null)
            return Result.NotFound();

        return Result.Success(new ProfileAggregateDto(new UserDto
        {
            Id = user.Id,
            Username = user.Username.Value,
            Bio = user.Bio?.Value,
            ImageUrl = user.ImageUrl?.Value,
            CreatedOn = user.CreatedOn,
        }));
    }
}
