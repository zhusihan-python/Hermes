using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Hermes.Features.Logs;

public partial class TextBoxWithTitle : UserControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<TextBoxWithTitle, string>(
        nameof(Title));

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<TextBoxWithTitle, string>(
        nameof(Text));

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<TextBoxWithTitle, string>(
            nameof(Watermark));

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public TextBoxWithTitle()
    {
        InitializeComponent();
    }
}