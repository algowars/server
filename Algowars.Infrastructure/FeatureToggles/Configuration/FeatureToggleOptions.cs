namespace Algowars.Infrastructure.FeatureToggles.Configuration;

public class FeatureToggleOptions
{
    public List<FeatureToggleConfigModel> Toggles { get; set; } = [];
}

public class FeatureToggleConfigModel
{
    public string FeatureName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Strategy { get; set; } = "AlwaysOn";
    public int? RolloutPercentage { get; set; }
}
