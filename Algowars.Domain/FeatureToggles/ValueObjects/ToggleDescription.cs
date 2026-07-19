namespace Algowars.Domain.FeatureToggles.ValueObjects;

public class ToggleDescription
{
    private const int MaxLength = 500;

    public string Value { get; private set; }

    public ToggleDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Toggle description cannot be empty.");

        if (value.Length > MaxLength)
            throw new ArgumentException($"Toggle description cannot exceed {MaxLength} characters.");

        Value = value.Trim();
    }

    public override bool Equals(object? obj) =>
        obj is ToggleDescription other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;
}
