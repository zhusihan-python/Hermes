using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Cipher.Types;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Material.Icons;
using System.Threading.Tasks;
using Hermes.Types;

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
    private readonly ISfcRepository _sfcRepository;

    public LoginViewModel(
        Session session,
        ISfcRepository sfcRepository) :
        base(
            Resources.txt_account,
            MaterialIconKind.Account,
            FeatureType.FreeAccess,
            0)
    {
        this._session = session;
        this._sfcRepository = sfcRepository;
        session.UserChanged += OnSessionUserChanged;
#if DEBUG
        LoginDebugUser();
#endif
    }

    [RelayCommand]
    private async Task Login()
    {
        IsLoggingIn = true;
        var user = await _sfcRepository.FindUser(this.UserName, this.Password);
        IsLoggedIn = !user.IsNull;
        _session.UpdateUser(user);
        if (!IsLoggedIn)
        {
            Messenger.Send(new ShowToastMessage(Resources.txt_error, Resources.msg_invalid_user_password,
                NotificationType.Error));
        }
        else
        {
            this.UserName = string.Empty;
            this.Password = string.Empty;
        }

        IsLoggingIn = false;
    }

    [RelayCommand]
    private void Logout()
    {
        this._session.Logout();
    }

    private void OnSessionUserChanged(User user)
    {
        this.User = user;
        this.IsLoggedIn = _session.IsLoggedIn;
    }

    partial void OnIsLoggedInChanged(bool value)
    {
        if (value)
        {
            LoginDebugUser();
        }
    }

    private void LoginDebugUser()
    {
        _session.UpdateUser(new DebugUser());
    }
}