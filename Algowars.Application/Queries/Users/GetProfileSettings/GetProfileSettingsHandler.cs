using Algowars.Application.Dtos.Users;
using Algowars.Domain.Users;
using Ardalis.Result;

namespace Algowars.Application.Queries.Users.GetProfileSettings;

internal sealed class GetProfileSettingsHandler(IUserRepository userRepository)
    : IQueryHandler<GetProfileSettingsQuery, ProfileSettingsDto>
{
    public async Task<Result<ProfileSettingsDto>> Handle(GetProfileSettingsQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindBySubAsync(request.Sub, cancellationToken);
        if (user is null)
            return Result.NotFound();

        return Result.Success(new ProfileSettingsDto(
            user.Username.Value,
            user.UsernameLastChangedAt,
            user.Bio?.Value ?? string.Empty));
    }
}
