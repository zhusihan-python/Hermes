using System;
using Avalonia.Controls;
using Avalonia;
using System.Collections;

namespace Hermes.Features.Controls;

public partial class ComboBoxEnum : UserControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<ComboBoxEnum, string>(
        nameof(Title));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<ItemsControl, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<ComboBoxEnum, object?>(
            nameof(SelectedItem));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ComboBoxEnum()
    {
        InitializeComponent();
    }
}