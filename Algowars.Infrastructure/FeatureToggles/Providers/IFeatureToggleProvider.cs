using Algowars.Domain.FeatureToggles.Entities;
using Algowars.Domain.FeatureToggles.Enums;

namespace Algowars.Infrastructure.FeatureToggles.Providers;

public interface IFeatureToggleProvider
{
    Task<FeatureToggle?> GetToggleAsync(FeatureName featureName);
    Task<IEnumerable<FeatureToggle>> GetAllTogglesAsync();
    Task RefreshAsync();
}
