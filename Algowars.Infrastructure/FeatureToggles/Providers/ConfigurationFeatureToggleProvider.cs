using Algowars.Domain.FeatureToggles.Entities;
using Algowars.Domain.FeatureToggles.Enums;
using Algowars.Domain.FeatureToggles.ValueObjects;
using Algowars.Infrastructure.FeatureToggles.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Algowars.Infrastructure.FeatureToggles.Providers;

public class ConfigurationFeatureToggleProvider : IFeatureToggleProvider
{
    private readonly IOptionsMonitor<FeatureToggleOptions> _optionsMonitor;
    private readonly ILogger<ConfigurationFeatureToggleProvider> _logger;
    private Dictionary<FeatureName, FeatureToggle> _toggleCache = [];

    public ConfigurationFeatureToggleProvider(
        IOptionsMonitor<FeatureToggleOptions> optionsMonitor,
        ILogger<ConfigurationFeatureToggleProvider> logger)
    {
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeCache();
    }

    public Task<FeatureToggle?> GetToggleAsync(FeatureName featureName)
    {
        _toggleCache.TryGetValue(featureName, out var toggle);
        return Task.FromResult(toggle);
    }

    public Task<IEnumerable<FeatureToggle>> GetAllTogglesAsync()
    {
        return Task.FromResult(_toggleCache.Values.AsEnumerable());
    }

    public Task RefreshAsync()
    {
        InitializeCache();
        return Task.CompletedTask;
    }

    private void InitializeCache()
    {
        FeatureToggleLog.RefreshStarted(_logger);

        var options = _optionsMonitor.CurrentValue;
        var newCache = new Dictionary<FeatureName, FeatureToggle>();

        if (options.Toggles == null || options.Toggles.Count == 0)
        {
            _toggleCache = newCache;
            FeatureToggleLog.RefreshCompleted(_logger, newCache.Count);
            return;
        }

        foreach (var config in options.Toggles)
        {
            try
            {
                var featureName = ParseFeatureName(config.FeatureName);
                var strategy = ParseToggleStrategy(config.Strategy);

                var toggle = new FeatureToggle(
                    featureName,
                    new FeatureToggleName(config.Name),
                    new ToggleDescription(config.Description),
                    config.IsEnabled,
                    strategy,
                    config.RolloutPercentage);

                newCache[featureName] = toggle;
            }
            catch (Exception ex)
            {
                FeatureToggleLog.RefreshFailed(_logger, config.FeatureName, ex);
            }
        }

        _toggleCache = newCache;
        FeatureToggleLog.RefreshCompleted(_logger, newCache.Count);
    }

    private static FeatureName ParseFeatureName(string value) =>
        Enum.TryParse<FeatureName>(value, ignoreCase: true, out var result)
            ? result
            : throw new ArgumentException($"Invalid feature name: {value}");

    private static ToggleStrategy ParseToggleStrategy(string value) =>
        Enum.TryParse<ToggleStrategy>(value, ignoreCase: true, out var result)
            ? result
            : ToggleStrategy.AlwaysOn;
}

public static partial class FeatureToggleLog
{
    [LoggerMessage(
        EventId = 6000,
        Level = LogLevel.Information,
        Message = "Feature toggle refresh started.")]
    public static partial void RefreshStarted(ILogger logger);

    [LoggerMessage(
        EventId = 6001,
        Level = LogLevel.Information,
        Message = "Feature toggle refresh completed. Loaded {ToggleCount} toggles.")]
    public static partial void RefreshCompleted(ILogger logger, int toggleCount);

    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Warning,
        Message = "Failed to parse feature toggle '{FeatureName}'.")]
    public static partial void RefreshFailed(ILogger logger, string featureName, Exception exception);
}
