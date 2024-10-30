using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using SukiUI.Controls;


namespace Hermes.Features;

public partial class MainWindowView : SukiWindow
{
    public new static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<SukiWindow, bool>(nameof(IsTitleBarVisible), defaultValue: true);

    public new bool IsTitleBarVisible
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

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (this.CanClose)
        {
            e.Cancel = false;
        }
        else
        {
            e.Cancel = true;
            (this.DataContext as MainWindowViewModel)?.LogoutCommand.Execute(null);
            this.Hide();
        }
    }

    private bool CanClose => (this.DataContext as MainWindowViewModel)?.CanClose ?? true;
}