using Algowars.Application.Users.Dtos;

namespace Algowars.Api.Responses.User;

public sealed record UserResponse(Guid Id, string Username, string? ImageUrl, DateTime? UsernameLastChangedAt, IReadOnlyList<string> Permissions, IReadOnlyList<string> Roles)
{
    public static UserResponse FromDto(UserDto dto, IReadOnlyList<string> permissions, IReadOnlyList<string> roles) => new(dto.Id, dto.Username, dto.ImageUrl, dto.UsernameLastChangedAt, permissions, roles);
}