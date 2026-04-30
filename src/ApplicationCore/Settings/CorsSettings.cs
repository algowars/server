namespace ApplicationCore.Settings;

public class CorsSettings : ISettings
{
    public static string SectionKey => "CorsSettings";
    public required IEnumerable<string> AllowedOrigins { get; set; }
}