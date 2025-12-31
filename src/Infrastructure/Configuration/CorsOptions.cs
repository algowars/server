namespace Infrastructure.Configuration;

public sealed class CorsOptions
{
    public string[] AllowedOrigins { get; init; } = [];
}
