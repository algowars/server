namespace Infrastructure.Configuration;

public sealed class Auth0Options
{
    public required string Audience { get; init; }

    public required string Domain { get; init; }

    public required Auth0ManagementOptions Management { get; init; }
}
