namespace Algowars.Api.RateLimiting;

/// Named rate limit policy keys shared between registration and controller attributes.
public static class WellKnownPolicies
{
    /// General API endpoints — browsing problems, user profiles, etc.
    /// 120 requests per minute per user / IP.
    public const string General = "rl:general";

    /// Submission endpoints — code is forwarded to Judge0 for execution.
    /// 10 requests per minute per authenticated user (sliding window).
    public const string Submissions = "rl:submissions";
}
