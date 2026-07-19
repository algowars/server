using Algowars.Domain.FeatureToggles.Entities;
using Algowars.Domain.FeatureToggles.Enums;
using Algowars.Domain.SeedWork;

namespace Algowars.Domain.FeatureToggles;

public interface IFeatureToggleRepository : IRepository
{
    Task<FeatureToggle?> GetByFeatureNameAsync(FeatureName featureName);
    Task<IEnumerable<FeatureToggle>> GetAllAsync();
    Task AddAsync(FeatureToggle featureToggle);
    Task UpdateAsync(FeatureToggle featureToggle);
    Task DeleteAsync(FeatureToggle featureToggle);
}
