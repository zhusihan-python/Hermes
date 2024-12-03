using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using SukiUI.Controls;


namespace Hermes.Features;

public partial class MainWindowView : SukiWindow
{
    public MainWindowView()
    {
        InitializeComponent();
        this.IsMenuVisible = false;
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