using Algowars.Domain.FeatureToggles.Enums;
using Algowars.Domain.FeatureToggles.ValueObjects;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.FeatureToggles.Entities;

public class FeatureToggle : Entity
{
    public FeatureName FeatureName { get; private set; }
    public FeatureToggleName Name { get; private set; }
    public ToggleDescription Description { get; private set; }
    public ToggleStrategy Strategy { get; private set; }
    public bool IsEnabled { get; private set; }
    public int? RolloutPercentage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private FeatureToggle()
    {
    }

    public FeatureToggle(
        FeatureName featureName,
        FeatureToggleName name,
        ToggleDescription description,
        bool isEnabled,
        ToggleStrategy strategy = ToggleStrategy.AlwaysOn,
        int? rolloutPercentage = null)
    {
        if (strategy == ToggleStrategy.PercentageRollout && (rolloutPercentage is null || rolloutPercentage < 0 || rolloutPercentage > 100))
            throw new ArgumentException("Rollout percentage must be between 0 and 100 when using PercentageRollout strategy.");

        FeatureName = featureName;
        Name = name;
        Description = description;
        IsEnabled = isEnabled;
        Strategy = strategy;
        RolloutPercentage = rolloutPercentage;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStrategy(ToggleStrategy strategy, int? rolloutPercentage = null)
    {
        if (strategy == ToggleStrategy.PercentageRollout && (rolloutPercentage is null || rolloutPercentage < 0 || rolloutPercentage > 100))
            throw new ArgumentException("Rollout percentage must be between 0 and 100 when using PercentageRollout strategy.");

        Strategy = strategy;
        RolloutPercentage = rolloutPercentage;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool EvaluateForUser(Guid userId)
    {
        if (!IsEnabled)
            return false;

        return Strategy switch
        {
            ToggleStrategy.AlwaysOn => true,
            ToggleStrategy.AlwaysOff => false,
            ToggleStrategy.PercentageRollout => EvaluatePercentageRollout(userId),
            _ => false
        };
    }

    private bool EvaluatePercentageRollout(Guid userId)
    {
        if (RolloutPercentage is null or <= 0)
            return false;

        if (RolloutPercentage >= 100)
            return true;

        var hash = userId.GetHashCode();
        var normalizedHash = Math.Abs(hash) % 100;
        return normalizedHash < RolloutPercentage;
    }
}
