namespace ApplicationCore.Settings;

public sealed class ConnectionStringsSettings : ISettings
{
    public static string SectionKey => "ConnectionStrings";
    public required string DefaultConnection { get; set; }
}
