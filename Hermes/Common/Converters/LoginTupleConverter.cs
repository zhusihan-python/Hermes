using Avalonia.Data.Converters;
using Hermes.Cipher.Types;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace Hermes.Common.Converters;

public static class TupleConverter
{
    public static readonly LoginTupleConverter LoginTupleConverter = new();
}

public class LoginTupleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var tuple = new Tuple<string, DepartmentType>(
            ((string)values[0]! ?? ""), (DepartmentType)(values[1] ?? 0));
        return (object)tuple;
    }
}