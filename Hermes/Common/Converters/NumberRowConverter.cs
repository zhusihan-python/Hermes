using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Hermes.Common.Converters;

public static class NumberRowConverters
{
    public static readonly NumberRowConverter Instance = new();
}

public class NumberRowConverter : IValueConverter
{
    public object Convert(object? value, Type TargetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is DataGridRow row)
            {
                return (row.GetIndex() + 1).ToString();
            }
            else
            {
                return "*";
            }
        }
        catch
        {
            return "*";
        }
    }

    public object ConvertBack(object? value, Type TargetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}