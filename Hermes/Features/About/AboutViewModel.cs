using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Language;
using Hermes.Types;
using Material.Icons;

namespace Hermes.Features.About;

public partial class AboutViewModel()
    : PageBase(
        Resources.txt_about,
        MaterialIconKind.InfoOutline,
        PermissionType.FreeAccess,
        100)
{
    public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    [ObservableProperty] private bool _dashBoardVisited;

    [RelayCommand]
    private void OpenDashboard()
    {
        DashBoardVisited = true;
    }
}