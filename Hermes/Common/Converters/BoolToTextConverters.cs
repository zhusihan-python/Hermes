using Avalonia.Data.Converters;
using Material.Icons;
using System.Globalization;
using System;
using Hermes.Language;
namespace Hermes.Common.Converters;
public static class BoolToTextConverters
{
    public static readonly BoolToTextConverter PassFail = new(Resources.txt_passed, Resources.txt_failed);
}
public class BoolToTextConverter(string trueText, string falseText) : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b) return null;
        return b ? trueText : falseText;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}