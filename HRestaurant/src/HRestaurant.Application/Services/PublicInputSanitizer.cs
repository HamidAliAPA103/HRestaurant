using System.Text;
using System.Text.RegularExpressions;

namespace HRestaurant.Services;

public static partial class PublicInputSanitizer
{
    public static string SanitizeRequired(
        string value,
        int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return Sanitize(value, maximumLength) ?? string.Empty;
    }

    public static string? Sanitize(
        string? value,
        int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var withoutTags = HtmlTagRegex().Replace(value, string.Empty);
        var builder = new StringBuilder(withoutTags.Length);
        var previousWasWhitespace = false;

        foreach (var character in withoutTags.Trim())
        {
            if (char.IsControl(character))
            {
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                }

                previousWasWhitespace = true;
                continue;
            }

            previousWasWhitespace = false;
            builder.Append(character);

            if (builder.Length == maximumLength)
            {
                break;
            }
        }

        return builder.ToString();
    }

    public static string NormalizePhone(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();
        var digits = new string(
            trimmed.Where(char.IsDigit).ToArray());

        return trimmed.StartsWith('+')
            ? $"+{digits}"
            : digits;
    }

    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();
}
