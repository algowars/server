using Algowars.Application.Services.FeatureToggles;
using Algowars.Api.Responses.FeatureToggles;
using Microsoft.AspNetCore.Mvc;

namespace Algowars.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureTogglesController : ControllerBase
{
    private readonly IFeatureToggleService _featureToggleService;

    public FeatureTogglesController(IFeatureToggleService featureToggleService)
    {
        _featureToggleService = featureToggleService ?? throw new ArgumentNullException(nameof(featureToggleService));
    }

    /// <summary>
    /// Get all enabled feature toggles for the client
    /// </summary>
    [HttpGet("enabled")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<FeatureTogglesResponse>> GetEnabledFeatures()
    {
        var toggles = await _featureToggleService.GetAllTogglesAsync();

        var response = new FeatureTogglesResponse
        {
            Toggles = toggles
                .Where(t => t.IsEnabled)
                .Select(t => new FeatureToggleResponse
                {
                    FeatureName = t.FeatureName.ToString(),
                    Name = t.Name.Value,
                    Description = t.Description.Value,
                    IsEnabled = t.IsEnabled,
                    Strategy = t.Strategy.ToString(),
                    RolloutPercentage = t.RolloutPercentage
                })
        };

        return Ok(response);
    }

    /// <summary>
    /// Get all feature toggles (admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<FeatureTogglesResponse>> GetAllFeatures()
    {
        var toggles = await _featureToggleService.GetAllTogglesAsync();

        var response = new FeatureTogglesResponse
        {
            Toggles = toggles.Select(t => new FeatureToggleResponse
            {
                FeatureName = t.FeatureName.ToString(),
                Name = t.Name.Value,
                Description = t.Description.Value,
                IsEnabled = t.IsEnabled,
                Strategy = t.Strategy.ToString(),
                RolloutPercentage = t.RolloutPercentage
            })
        };

        return Ok(response);
    }

    /// <summary>
    /// Check if a specific feature is enabled for the current user
    /// </summary>
    [HttpGet("check/{featureName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<dynamic>> CheckFeature(string featureName)
    {
        if (!Enum.TryParse<Algowars.Domain.FeatureToggles.Enums.FeatureName>(featureName, ignoreCase: true, out var parsedFeatureName))
        {
            return BadRequest(new { error = $"Invalid feature name: {featureName}" });
        }

        var isEnabled = await _featureToggleService.IsEnabledAsync(parsedFeatureName);

        return Ok(new { featureName, isEnabled });
    }
}
