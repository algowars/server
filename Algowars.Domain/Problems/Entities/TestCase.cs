using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Entities;

public sealed class TestCase : Entity
{
    private TestCase() { }

    internal TestCase(string input, string expectedOutput, bool isHidden = true)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        ExpectedOutput = expectedOutput ?? throw new ArgumentNullException(nameof(expectedOutput));
        IsHidden = isHidden;
    }

    public string ExpectedOutput { get; private set; } = string.Empty;
    public string Input { get; private set; } = string.Empty;
    public bool IsHidden { get; private set; }
}
