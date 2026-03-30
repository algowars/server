namespace PublicApi.Attributes;

[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Class,
    AllowMultiple = false,
    Inherited = true
)]
public sealed class GlobalRateLimitAttribute(int count, int seconds) : Attribute
{
    public int Count { get; } = count;
    public int Seconds { get; } = seconds;

    public string PolicyName => $"Global_{Count}:{Seconds}";
}
