using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Material.Icons;

namespace Hermes.Features.Controls;

public partial class ButtonIcon : UserControl
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<ButtonIcon, ICommand?>(
            nameof(Command));

    public static readonly StyledProperty<MaterialIconKind> KindProperty =
        AvaloniaProperty.Register<ButtonIcon, MaterialIconKind>(
            nameof(Kind));

    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<ButtonIcon, double>(
        nameof(IconSize), defaultValue: 20);

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public MaterialIconKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ButtonIcon()
    {
        this.Width = 35;
        this.Height = 35;
        InitializeComponent();
    }
}