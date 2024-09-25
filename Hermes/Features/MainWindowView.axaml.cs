using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SukiUI.Controls;


namespace Hermes.Features;

public partial class MainWindowView : SukiWindow
{
    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsTitleBarVisible), defaultValue: true);

    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    public MainWindowView()
    {
        InitializeComponent();
#if DEBUG
        this.IsMenuVisible = true;
#endif
        IsTitleBarVisible = false;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsMenuVisible = !IsMenuVisible;
    }

    private void MakeFullScreenPressed(object? sender, PointerPressedEventArgs e)
    {
        WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
        IsTitleBarVisible = WindowState != WindowState.FullScreen;
    }
}