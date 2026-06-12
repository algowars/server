using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problems.Entities;

public sealed class Example : Entity
{
    private Example() { }

    internal Example(string input, string output, string? explanation = null)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        Output = output ?? throw new ArgumentNullException(nameof(output));
        Explanation = explanation;
    }

    public string? Explanation { get; private set; }
    public string Input { get; private set; } = string.Empty;
    public string Output { get; private set; } = string.Empty;
}
