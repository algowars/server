using Algowars.Domain.SeedWork;

namespace Algowars.Domain.TestSuites.Entities;

public sealed class TestCaseInput : Entity
{
    internal TestCaseInput(string value, string valueType)
    {
        Value = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Value must not be empty.", nameof(value));

        ValueType = !string.IsNullOrWhiteSpace(valueType)
            ? valueType
            : throw new ArgumentException("Value type must not be empty.", nameof(valueType));
    }

    private TestCaseInput() { }

    public string Value { get; private set; } = null!;
    public string ValueType { get; private set; } = null!;
}
