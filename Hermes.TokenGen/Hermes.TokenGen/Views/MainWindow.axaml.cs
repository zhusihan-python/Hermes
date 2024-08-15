using Avalonia.Controls;
using Hermes.TokenGen.Services;

namespace Hermes.TokenGen.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var desktopWindowService = new DesktopWindowService
        {
            IsActive = true
        };
        InitializeComponent();
        Closed += (sender, args) => desktopWindowService.IsActive = false;
    }
}