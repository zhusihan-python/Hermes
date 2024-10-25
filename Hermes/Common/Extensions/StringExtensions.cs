using System;

namespace Hermes.Common.Extensions;

public static class StringExtensions
{
    public static string ExtractFirstLetter(this string value, char letter)
    {
        if (value.StartsWith(letter.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            value = value[1..];
        }

        return value.ToUpper();
    }

    public static T? ToEnum<T>(this string value) where T : struct
    {
        return Enum.TryParse<T>(value, out var result) ? result : null;
    }
}