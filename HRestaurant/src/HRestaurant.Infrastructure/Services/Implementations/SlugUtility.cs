using System.Globalization;
using System.Text;

namespace HRestaurant.Services.Implementations;

internal static class SlugUtility
{
    public static string Create(string value, string fallback, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(fallback);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 1);

        var transliterated = value
            .Trim()
            .ToLowerInvariant()
            .Replace('ə', 'e')
            .Replace('ı', 'i')
            .Replace('ö', 'o')
            .Replace('ü', 'u')
            .Replace('ş', 's')
            .Replace('ç', 'c')
            .Replace('ğ', 'g');
        var normalized = transliterated.Normalize(
            NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasHyphen = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character)
                == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasHyphen = false;
            }
            else if (!previousWasHyphen && builder.Length > 0)
            {
                builder.Append('-');
                previousWasHyphen = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (slug.Length == 0)
        {
            slug = fallback;
        }

        return slug[..Math.Min(slug.Length, maxLength)];
    }
}
