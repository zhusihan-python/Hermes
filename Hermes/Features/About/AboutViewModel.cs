using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace Hermes.Features.About;

public partial class AboutViewModel()
    : PageBase("About", MaterialIconKind.InfoOutline, 0, 100)
{
    public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    [ObservableProperty] private bool _dashBoardVisited;

    [RelayCommand]
    private void OpenDashboard()
    {
        DashBoardVisited = true;
    }
}