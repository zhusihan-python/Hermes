using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;
using Hermes.Language;

namespace Hermes.Common.Extensions;

public static class EnumExtensions
{
    public static string ToUpperString(this Enum value)
    {
        return value.ToString().ToUpper();
    }

    public static string ToTranslatedString(this Enum value)
    {
        return Resources.ResourceManager.GetString(value.ToResxString()) ?? value.ToString();
    }

    private static string ToResxString(this Enum value)
    {
        return "enum_" + value.ToString().ToLower();
    }

    public static string GetDescription(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        var attributes =
            fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[] ??
            [];
        return attributes.Length != 0 ? attributes.First().Description : "";
    }

    public static List<string> GetEnumValues<T>() where T : Enum
    {
        return GetValues<T>().Select(x => x.ToString()).ToList();
    }

    public static T[] GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
    }
}