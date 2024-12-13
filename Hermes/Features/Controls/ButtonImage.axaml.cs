using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using System.Windows.Input;

namespace Hermes.Features.Controls;

public partial class ButtonImage : UserControl
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<ButtonImage, ICommand?>(
            nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<ButtonImage, object?>(
            nameof(CommandParameter));

    public static readonly StyledProperty<Bitmap?> NormalBackgroundProperty =
        AvaloniaProperty.Register<ButtonImage, Bitmap?>(
            nameof(NormalBackground));

    public static readonly StyledProperty<Bitmap?> PressedBackgroundProperty =
        AvaloniaProperty.Register<ButtonImage, Bitmap?>(
            nameof(PressedBackground));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public Bitmap? NormalBackground
    {
        get => GetValue(NormalBackgroundProperty);
        set => SetValue(NormalBackgroundProperty, value);
    }

    public Bitmap? PressedBackground
    {
        get => GetValue(PressedBackgroundProperty);
        set => SetValue(PressedBackgroundProperty, value);
    }
    public ButtonImage()
    {
        this.Width = 120;
        this.Height = 120;
        InitializeComponent();
    }
}
