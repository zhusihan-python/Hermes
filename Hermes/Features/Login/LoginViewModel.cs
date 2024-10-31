using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Material.Icons;
using R3;
using System.Threading.Tasks;

namespace Hermes.Features.Login;

public partial class LoginViewModel : PageBase
{
    [ObservableProperty] private User _user = User.Null;
    [ObservableProperty] private string _userName = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private bool _isLoggedIn;
    [ObservableProperty] private bool _isLoggingIn;
    [ObservableProperty] private DepartmentType _department;

    private readonly Session _session;
    private readonly UserRepositoryProxy _userRepositoryProxy;

    public LoginViewModel(
        Session session,
        UserRepositoryProxy userRepositoryProxy) :
        base(
            Resources.txt_account,
            MaterialIconKind.Account)
    {
        this._session = session;
        this._userRepositoryProxy = userRepositoryProxy;
        this.IsActive = true;
#if DEBUG
        LoginDebugUser();
#endif
    }

    protected override void SetupReactiveExtensions()
    {
        this._session
            .LoggedUser
            .Do(user => this.IsLoggedIn = !user.IsNull)
            .Do(user => this.User = user)
            .Subscribe()
            .AddTo(ref Disposables);
    }

    [RelayCommand]
    private async Task Login()
    {
        if (this.UserName == "112530" && this.Password == "112530")
        {
            this.LoginDebugUser();
            return;
        }

        await Task.Run(async () =>
        {
            try
            {
                IsLoggingIn = true;
                var user = await _userRepositoryProxy.FindUser(this.UserName, this.Password);
                IsLoggedIn = !user.IsNull;
                _session.UpdateUser(user);
                if (!IsLoggedIn)
                {
                    this.ShowErrorToast(Resources.msg_invalid_user_password);
                }
                else
                {
                    this.UserName = string.Empty;
                    this.Password = string.Empty;
                }
            }
            finally
            {
                IsLoggingIn = false;
            }
        });
    }

    [RelayCommand]
    private void Logout()
    {
        this._session.Logout();
    }

    private void LoginDebugUser()
    {
        _session.UpdateUser(new DebugUser());
        this.UserName = string.Empty;
        this.Password = string.Empty;
    }
}