namespace Algowars.Api;

internal static class LoggingEventIds
{
    internal static class Accounts
    {
        public const int ContextMissingSub = 1000;
        public const int ContextResolveFailed = 1001;
        public const int ContextPublicEndpoint = 1002;
    }

    internal static class Exceptions
    {
        public const int UnhandledException = 5000;
        public const int UnhandledExceptionWithPath = 5001;
    }

    internal static class FeatureToggles
    {
        public const int ToggleRefreshStarted = 6000;
        public const int ToggleRefreshCompleted = 6001;
        public const int ToggleRefreshFailed = 6002;
        public const int ToggleNotFound = 6003;
    }
}