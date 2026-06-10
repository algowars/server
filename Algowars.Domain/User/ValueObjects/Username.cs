using Algowars.Domain.User.Exceptions;

namespace Algowars.Domain.User.ValueObjects;

public sealed record Username
{
    public static readonly int MaxLength = 20;
    public static readonly int MinLength = 1;

    public string Value { get; }

    public Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidUsernameException("Username cannot be null or whitespace.");

        if (value.Length < MinLength || value.Length > MaxLength)
            throw new InvalidUsernameException($"Username must be between {MinLength} and {MaxLength} characters.");

        Value = value;
    }

    public static implicit operator string(Username username) => username.Value;
    public override string ToString() => Value;
}