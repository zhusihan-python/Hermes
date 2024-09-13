using Avalonia.Data.Converters;
using Material.Icons;
using System.Globalization;
using System;

namespace Hermes.Common.Converters;

public static class DateConverters
{
    public static readonly DateToShortStringConverter ToShortString = new();
    public static readonly DateToIsVisibleConverter ToIsVisible = new();
    public static readonly DateToNotIsVisibleConverter ToNotIsVisible = new();
}

public class DateToShortStringConverter() : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not DateTime dateTime ? "" : $"{dateTime:dd/MM/yyyy HH:mm}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DateToNotIsVisibleConverter() : DateToIsVisibleConverter
{
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = base.Convert(value, targetType, parameter, culture);
        if (result is not bool isVisible) return false;
        return !isVisible;
    }
}

public class DateToIsVisibleConverter() : IValueConverter
{
    public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return false;
        return date != DateTime.MinValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}