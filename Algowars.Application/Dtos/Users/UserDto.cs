namespace Algowars.Application.Dtos.Users;

public sealed record UserDto
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public string? ImageUrl { get; init; }
    public string? Bio { get; init; }
    public required DateTime CreatedOn { get; init; }
    public DateTime? UsernameLastChangedAt { get; init; }
}
