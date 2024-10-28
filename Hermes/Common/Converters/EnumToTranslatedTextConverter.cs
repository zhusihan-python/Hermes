using Avalonia.Data.Converters;
using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Types;
using System.Globalization;
using System;
using Hermes.Language;

namespace Hermes.Common.Converters;

public static class EnumToTranslatedTextConverters
{
    public static readonly EnumToTranslatedTextConverter<SfcResponseType> SfcResponseTypeConverter = new();
    public static readonly EnumToTranslatedTextConverter<DepartmentType> DepartmentTypeConverter = new();
    public static readonly EnumToTranslatedTextConverter<PackageStatusType> PackageStatusTypeConverter = new();
    public static readonly EnumToTranslatedTextConverter<StateType> StateTypeTypeConverter = new();
    public static readonly EnumToTranslatedTextConverter<StatusType> StatusTypeConverter = new();
}

public class EnumToTranslatedTextConverter<T>() : IValueConverter where T : Enum
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return Resources.txt_all;
        return value is not T myEnum ? default(object?) : myEnum.ToTranslatedString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}