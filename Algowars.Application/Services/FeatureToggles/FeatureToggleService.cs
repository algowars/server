using Algowars.Domain.FeatureToggles.Entities;
using Algowars.Domain.FeatureToggles.Enums;
using Algowars.Infrastructure.FeatureToggles.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace Algowars.Application.Services.FeatureToggles;

public interface IFeatureToggleService
{
    Task<bool> IsEnabledAsync(FeatureName featureName);
    Task<bool> IsEnabledForUserAsync(FeatureName featureName, Guid userId);
    Task<FeatureToggle?> GetToggleAsync(FeatureName featureName);
    Task<IEnumerable<FeatureToggle>> GetAllTogglesAsync();
    Task RefreshAsync();
}

public class FeatureToggleService : IFeatureToggleService
{
    private readonly IFeatureToggleProvider _provider;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "feature_toggle_";
    private const string AllTogglesKey = "all_feature_toggles";

    public FeatureToggleService(IFeatureToggleProvider provider, IMemoryCache cache)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<bool> IsEnabledAsync(FeatureName featureName)
    {
        var toggle = await GetToggleAsync(featureName);
        return toggle?.IsEnabled ?? false;
    }

    public async Task<bool> IsEnabledForUserAsync(FeatureName featureName, Guid userId)
    {
        var toggle = await GetToggleAsync(featureName);
        return toggle?.EvaluateForUser(userId) ?? false;
    }

    public async Task<FeatureToggle?> GetToggleAsync(FeatureName featureName)
    {
        var cacheKey = $"{CacheKeyPrefix}{featureName}";

        if (_cache.TryGetValue(cacheKey, out FeatureToggle? cachedToggle))
        {
            return cachedToggle;
        }

        var toggle = await _provider.GetToggleAsync(featureName);

        if (toggle != null)
        {
            _cache.Set(cacheKey, toggle, _cacheDuration);
        }

        return toggle;
    }

    public async Task<IEnumerable<FeatureToggle>> GetAllTogglesAsync()
    {
        if (_cache.TryGetValue(AllTogglesKey, out IEnumerable<FeatureToggle>? cachedToggles))
        {
            return cachedToggles!;
        }

        var toggles = await _provider.GetAllTogglesAsync();
        var togglesList = toggles.ToList();

        _cache.Set(AllTogglesKey, togglesList, _cacheDuration);

        return togglesList;
    }

    public async Task RefreshAsync()
    {
        // Clear cache
        _cache.Remove(AllTogglesKey);
        foreach (var featureName in Enum.GetValues<FeatureName>())
        {
            _cache.Remove($"{CacheKeyPrefix}{featureName}");
        }

        // Refresh from provider
        await _provider.RefreshAsync();
    }
}
