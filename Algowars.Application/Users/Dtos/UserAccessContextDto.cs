namespace Algowars.Application.Users.Dtos;

public sealed record UserAccessContextDto(
    UserDto User,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles);