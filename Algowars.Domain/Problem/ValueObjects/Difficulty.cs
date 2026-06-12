using Algowars.Domain.Problem.Enums;
using Algowars.Domain.Problem.Exceptions;

namespace Algowars.Domain.Problem.ValueObjects;

public sealed record Difficulty
{
    public Difficulty(int value)
    {
        if (value < MinValue)
            throw new InvalidDifficultyException($"Difficulty cannot be less than {MinValue}.");

        Value = value;
    }

    public override string ToString() => $"{Value} ({Tier})";

    public static readonly int EasyMax = 1000;
    public static readonly int EasyMin = 0;
    public static readonly int HardMin = 2001;
    public static readonly int MediumMax = 2000;
    public static readonly int MediumMin = 1001;
    public static readonly int MinValue = 0;

    public DifficultyTier Tier => Value switch
    {
        <= 1000 => DifficultyTier.Easy,
        <= 2000 => DifficultyTier.Medium,
        _ => DifficultyTier.Hard
    };

    public int Value { get; }
}
