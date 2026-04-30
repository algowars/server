namespace ApplicationCore.Settings;

public class MediatRSettings : ISettings
{
    public static string SectionKey => "MediatR";

    public string? LicenseKey { get; set; }
}