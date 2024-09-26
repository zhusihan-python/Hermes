using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Common.Converters;

public static class PackageStatusToIconConverters
{
    public static readonly PackageStatusToIconConverter Instance = new();
}

public class PackageStatusToIconConverter : IValueConverter
{
    public object Convert(object? value, Type TargetType, object? parameter, CultureInfo culture)
    {
        var icon = (PackageStatusType)value! switch
        {
            PackageStatusType.Scanned => MaterialIconKind.QrcodeScan,
            PackageStatusType.Open => MaterialIconKind.PackageVariant,
            PackageStatusType.Loaded => MaterialIconKind.PackageVariantClosed,
            PackageStatusType.InUse => MaterialIconKind.PackageUp,
            PackageStatusType.Used => MaterialIconKind.PackageCheck,
            _ => MaterialIconKind.AlertCircle
        };

        return icon;
    }

    public object ConvertBack(object? value, Type TargetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}