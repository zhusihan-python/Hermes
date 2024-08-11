using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Language;
using Hermes.Models;
using Material.Icons;

namespace Hermes.Features.Login;

public partial class LoginViewModel(
    Session session
    )
    : PageBase(Resources.txt_account, MaterialIconKind.Account, 0, 0)
{
    [ObservableProperty] private bool _isLoggedIn;
    [ObservableProperty] private bool _isLoggingIn;

    [RelayCommand]
    private async Task Login()
    {
        IsLoggingIn = true;
        await Task.Delay(1000);
        IsLoggedIn = !IsLoggedIn;
        IsLoggingIn = false;
    }

    partial void OnIsLoggedInChanged(bool value)
    {
        session.UpdateUser(new User()
        {
            UpdateLevel = value ? 1 : 0,
            ViewLevel = value ? 1 : 0,
            CanExit = value
        });
    }
}