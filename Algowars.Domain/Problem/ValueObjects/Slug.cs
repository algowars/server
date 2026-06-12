using System.Text.RegularExpressions;
using Algowars.Domain.Problem.Exceptions;

namespace Algowars.Domain.Problem.ValueObjects;

public sealed record Slug
{
    private static readonly Regex ValidSlugPattern = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidSlugException("Slug cannot be empty.");

        if (value.Length < MinLength || value.Length > MaxLength)
            throw new InvalidSlugException($"Slug must be between {MinLength} and {MaxLength} characters.");

        if (!ValidSlugPattern.IsMatch(value))
            throw new InvalidSlugException("Slug must be lowercase, contain only letters, numbers, and hyphens, and cannot start or end with a hyphen.");

        Value = value;
    }

    public static Slug FromTitle(Title title)
    {
        var slug = title.Value.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return new Slug(slug);
    }

    public static implicit operator string(Slug slug) => slug.Value;
    public override string ToString() => Value;

    public static readonly int MaxLength = 200;
    public static readonly int MinLength = 3;
    public string Value { get; }
}
