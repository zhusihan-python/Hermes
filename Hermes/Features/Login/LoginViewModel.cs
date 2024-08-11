using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Hermes.Features.Login;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isLoggingIn;
    
    [RelayCommand]
    private async Task Login()
    {
        IsLoggingIn = true;
        await Task.Delay(1000);
        IsLoggingIn = false;
    }
}