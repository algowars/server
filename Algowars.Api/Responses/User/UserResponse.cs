using Algowars.Application.Users.Dtos;

namespace Algowars.Api.Responses.User;

public sealed record UserResponse(Guid Id, string Username, string? ImageUrl)
{
    public static UserResponse FromDto(UserDto dto) => new(dto.Id, dto.Username, dto.ImageUrl);
}
