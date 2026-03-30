namespace PublicApi.Attributes;

[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Class,
    AllowMultiple = false,
    Inherited = true
)]
public sealed class UserRateLimitAttribute(int count, int seconds) : Attribute
{
    public int Count { get; } = count;
    public int Seconds { get; } = seconds;

    public string PolicyName => $"User_{Count}:{Seconds}";
}
