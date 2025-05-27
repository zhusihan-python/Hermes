using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Hermes.Language;
using Hermes.Models;

namespace Hermes.Common.Converters;

public static class DoctorToTextConverters
{
    public static readonly DoctorToTextConverter<Doctor> Generic = new();
}

public class DoctorToTextConverter<T>() : IValueConverter where T : Doctor
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return Resources.txt_all;
        return value is not T doc ? default(object?) : doc.Name;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}