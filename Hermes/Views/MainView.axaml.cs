using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Material.Styles.Controls;

namespace Hermes.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        SnackbarHost.Post("See ya next time, user!", null, DispatcherPriority.Normal);
    }
}