using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Material.Icons;

namespace Hermes.TokenGen.Views.Controls;

public class ButtonIcon : Button
{
    public static readonly StyledProperty<int> IconSizeProperty =
        AvaloniaProperty.Register<ButtonIcon, int>(nameof(IconSize), defaultValue: 25);

    public int IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly StyledProperty<bool> IsTextVisibleProperty =
        AvaloniaProperty.Register<ButtonIcon, bool>(nameof(IsTextVisible), defaultValue: false);

    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<ButtonIcon, string>(nameof(Text), defaultValue: "");

    public string Text
    {
        get => GetValue(TextProperty);
        set
        {
            IsTextVisible = !string.IsNullOrWhiteSpace(value);
            SetValue(TextProperty, value);
        }
    }

    public static readonly StyledProperty<MaterialIconKind> IconKindProperty =
        AvaloniaProperty.Register<ButtonIcon, MaterialIconKind>(nameof(IconKind),
            defaultValue: MaterialIconKind.Abacus);

    public MaterialIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }
}