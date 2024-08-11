using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Hermes.Types;

namespace Hermes.Common.Converters;

public static class ToupleConverter
{
    public static readonly LoginToupleConverter LoginToupleConverter = new();
}

public class LoginToupleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var tuple = new Tuple<string, DepartmentType>(
            ((string)values[0]! ?? ""), (DepartmentType)(values[1] ?? 0));
        return (object)tuple;
    }
}