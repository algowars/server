using Algowars.Infrastructure.FeatureToggles.Providers;
using Microsoft.Extensions.Logging;

namespace Algowars.Api.Middleware;

public class InitializeFeatureTogglesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFeatureToggleProvider _provider;
    private readonly ILogger<InitializeFeatureTogglesMiddleware> _logger;
    private bool _initialized = false;
    private readonly object _lockObject = new object();

    public InitializeFeatureTogglesMiddleware(
        RequestDelegate next,
        IFeatureToggleProvider provider,
        ILogger<InitializeFeatureTogglesMiddleware> logger)
    {
        _next = next;
        _provider = provider;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_initialized)
        {
            lock (_lockObject)
            {
                if (!_initialized)
                {
                    try
                    {
                        // Initialize provider on first request
                        _provider.RefreshAsync().Wait();
                        _initialized = true;
                    }
                    catch (Exception ex)
                    {
                        FeatureToggleMiddlewareLog.InitializationFailed(_logger, ex);
                    }
                }
            }
        }

        await _next(context);
    }
}

public static class FeatureTogglesMiddlewareExtensions
{
    public static IApplicationBuilder UseFeatureToggles(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<InitializeFeatureTogglesMiddleware>();
    }
}

public static partial class FeatureToggleMiddlewareLog
{
    [LoggerMessage(
        EventId = 6002,
        Level = LogLevel.Error,
        Message = "Failed to initialize feature toggles.")]
    public static partial void InitializationFailed(ILogger logger, Exception exception);
}
