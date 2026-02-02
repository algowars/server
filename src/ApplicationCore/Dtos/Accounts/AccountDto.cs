namespace ApplicationCore.Dtos.Accounts;

public sealed record AccountDto
{
    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public string? ImageUrl { get; init; }

    public IEnumerable<string> Permissions { get; init; } = [];

    public required DateTime CreatedOn { get; init; }
}