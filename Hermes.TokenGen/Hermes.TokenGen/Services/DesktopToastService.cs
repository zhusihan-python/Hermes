using SukiUI.Controls;

namespace Hermes.TokenGen.Services;

public class DesktopToastService : ToastService
{
    protected override void ShowToast(string title, string message)
    {
        SukiHost.ShowToast(title, message);
    }
}