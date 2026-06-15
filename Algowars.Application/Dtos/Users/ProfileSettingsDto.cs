namespace Algowars.Application.Dtos.Users;

public sealed record ProfileSettingsDto(string Username, DateTime? UsernameLastChangedAt, string Bio);
