using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ApplicationCore.Interfaces.Services;

namespace Infrastructure.Services;

public sealed partial class SlugService : ISlugService
{
    public string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        string normalized = input
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c))
            .ToString()
            .Normalize(NormalizationForm.FormC);

        normalized = normalized.ToLowerInvariant();

        normalized = NonAlphaNumericRegex().Replace(normalized, "");
        normalized = WhitespaceRegex().Replace(normalized, "-").Trim('-');
        normalized = MultipleDashRegex().Replace(normalized, "-");

        return normalized;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex MultipleDashRegex();
}
