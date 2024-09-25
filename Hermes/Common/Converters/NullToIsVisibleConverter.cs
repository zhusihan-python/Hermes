using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Hermes.Common.Converters;

public static class GenericConverters
{
    public static readonly NullToIsVisibleConverter NullToIsVisibleConverter = new();
    public static readonly NotNullToIsVisibleConverter NotNullToIsVisibleConverter = new();
}

public class NotNullToIsVisibleConverter : NullToIsVisibleConverter
{
    public override object Convert(object? value, Type TargetType, object? parameter, CultureInfo culture)
    {
        if (base.Convert(value, TargetType, parameter, culture) is not bool val) return false;
        return !val;
    }
}

public class NullToIsVisibleConverter : IValueConverter
{
    public virtual object Convert(object? value, Type TargetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object ConvertBack(object? value, Type TargetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}