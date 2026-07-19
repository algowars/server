namespace Algowars.Api.Responses.FeatureToggles;

public class FeatureToggleResponse
{
    public string FeatureName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Strategy { get; set; } = string.Empty;
    public int? RolloutPercentage { get; set; }
}

public class FeatureTogglesResponse
{
    public IEnumerable<FeatureToggleResponse> Toggles { get; set; } = [];
}
