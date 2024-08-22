using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Material.Icons;
using System.Threading.Tasks;


namespace Hermes.Features.Login;

public partial class LoginViewModel : PageBase
{
    [ObservableProperty] private User _user = User.Null;
    [ObservableProperty] private string _token;
    [ObservableProperty] private bool _isLoggedIn;
    [ObservableProperty] private bool _isLoggingIn;
    [ObservableProperty] private DepartmentType _department;
    private readonly Session _session;
    private readonly UserRepository _userRepository;
    public static DepartmentType[] Departments => EnumExtensions.GetValues<DepartmentType>();

    public LoginViewModel(
        Session session,
        UserRepository userRepository) :
        base(Resources.txt_account, MaterialIconKind.Account, 0, 0)
    {
        this._session = session;
        this._userRepository = userRepository;
        session.UserChanged += OnSessionUserChanged;
#if DEBUG
        LoginDebugUser();
#endif
    }

    [RelayCommand]
    private async Task Login()
    {
        IsLoggingIn = true;
        var user = _userRepository.GetUser(token: this.Token, department: this.Department);
        IsLoggedIn = !user.IsNull;
        _session.UpdateUser(user);
        await Task.Delay(1000);
        IsLoggingIn = false;
        this.Token = string.Empty;
        if (user.IsNull)
        {
            Messenger.Send(new ShowToastMessage(Resources.txt_invalid_token, Resources.msg_invalid_token));
        }
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
        _session.UpdateUser(_userRepository.GetDebugUser());
    }
}