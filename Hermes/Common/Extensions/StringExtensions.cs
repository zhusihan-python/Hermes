using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;
using Hermes.Language;

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
}