namespace Algowars.Application.Users.Dtos;

public sealed record UserDto(Guid Id, string Sub, string Username, string? ImageUrl);
