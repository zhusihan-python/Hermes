using System.Globalization;
using Avalonia.Data.Converters;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Types;

namespace Hermes.Cipher.Converters;

public static class EnumToTranslatedTextConverters
{
    public static readonly EnumToTranslatedTextConverter<DepartmentType> DepartmentTypeConverter = new();
}

public class EnumToTranslatedTextConverter<T>() : IValueConverter where T : Enum
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not T myEnum ? default(object?) : myEnum.ToTranslatedString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}