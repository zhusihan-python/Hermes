using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Hermes.Types;
using System;
using System.Globalization;

namespace Hermes.Common.Converters;



public class LEDStateToImageConverter : IValueConverter
{
    public static LEDStateToImageConverter Instance { get; } = new();
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var imagePath = (LEDState)value! switch
        {
            LEDState.Disconnect => "avares://Hermes/AppData/Assets/LED_Disconnect.png",
            LEDState.Normal => "avares://Hermes/AppData/Assets/LED_Normal.png",
            LEDState.Warning => "avares://Hermes/AppData/Assets/LED_Warning.png",
            LEDState.Error => "avares://Hermes/AppData/Assets/LED_Error.png",
            _ => "avares://Hermes/AppData/Assets/LED_Disconnect.png"
        };
        Uri uri;
        uri = new Uri(imagePath);
        var asset = AssetLoader.Open(uri);

        return new Bitmap(asset);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}