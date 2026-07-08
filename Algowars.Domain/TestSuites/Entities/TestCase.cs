using Algowars.Domain.SeedWork;

namespace Algowars.Domain.TestSuites.Entities;

public sealed class TestCase : Entity
{
    internal TestCase(string name, string? description = null)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name must not be empty.", nameof(name));

        Description = description;
    }

    public TestCaseInput AddInput(string value, string valueType)
    {
        var input = new TestCaseInput(value, valueType);
        _inputs.Add(input);
        return input;
    }

    public TestCaseExpectedOutput AddExpectedOutput(string value, string valueType)
    {
        var output = new TestCaseExpectedOutput(value, valueType);
        _expectedOutputs.Add(output);
        return output;
    }

    private TestCase() { }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public IReadOnlyCollection<TestCaseInput> Inputs => _inputs.AsReadOnly();
    public IReadOnlyCollection<TestCaseExpectedOutput> ExpectedOutputs => _expectedOutputs.AsReadOnly();

    private readonly List<TestCaseInput> _inputs = [];
    private readonly List<TestCaseExpectedOutput> _expectedOutputs = [];
}
