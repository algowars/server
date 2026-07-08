namespace Algowars.Application.Users.Dtos;

public sealed record UpsertUserDto(string? Username, string? ImageUrl, string? Bio);