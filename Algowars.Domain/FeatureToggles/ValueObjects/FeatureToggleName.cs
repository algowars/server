namespace Algowars.Domain.FeatureToggles.ValueObjects;

public class FeatureToggleName
{
    private const int MaxLength = 100;

    public string Value { get; private set; }

    public FeatureToggleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Feature toggle name cannot be empty.");

        if (value.Length > MaxLength)
            throw new ArgumentException($"Feature toggle name cannot exceed {MaxLength} characters.");

        Value = value.Trim();
    }

    public override bool Equals(object? obj) =>
        obj is FeatureToggleName other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;
}
