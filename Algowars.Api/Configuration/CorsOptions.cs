using Algowars.Application.Settings;

namespace Algowars.Api.Settings;

public sealed class CorsOptions : IOption
{
    public static string SectionName => "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
