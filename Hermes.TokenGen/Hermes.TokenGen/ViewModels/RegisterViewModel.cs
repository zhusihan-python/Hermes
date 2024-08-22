using Avalonia.SimpleRouter;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Types;
using Hermes.Cipher;
using Hermes.TokenGen.Common.Messages;
using Hermes.TokenGen.Models;
using Hermes.TokenGen.Repositories;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;

namespace Hermes.TokenGen.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly Regex _employeeNumberRegex = new(@"^[0-9]{6,}$");

    [ObservableProperty] private bool _isManager;
    [ObservableProperty] private bool _isLoggedIn;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _employeeNumber = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _adminToken = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private DepartmentType _department;

    private readonly TokenGenerator _tokenGenerator;
    private readonly UserRepository _userRepository;
    private readonly Session _session;
    private readonly HistoryRouter<ViewModelBase> _router;

    public static DepartmentType[] Departments => EnumExtensions.GetValues<DepartmentType>();

    public RegisterViewModel(
        Session session,
        TokenGenerator tokenGenerator,
        UserRepository userRepository,
        HistoryRouter<ViewModelBase> router)
    {
        _session = session;
        _tokenGenerator = tokenGenerator;
        _userRepository = userRepository;
        _router = router;
        IsLoggedIn = _session.IsUserLoggedIn;
        if (IsLoggedIn)
        {
            IsManager = _session.IsUserManager;
            EmployeeNumber = _session.UserNumber;
            Department = _session.UserDepartment;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task Register()
    {
#if !DEBUG
        if (!_tokenGenerator.TryDecode(
                AdminToken.ToUpper(),
                (int)DepartmentType.Admin,
                DateOnly.FromDateTime(DateTime.Now),
                out var _))
        {
            Messenger.Send(new ShowToastMessage("Invalid admin token", "Please verify the admin token"));
            return;
        }
#endif
        _session.User = new User()
        {
            IsManager = IsManager,
            Number = EmployeeNumber,
            Department = Department
        };
        await _userRepository.SaveUser(_session.User);
        _router.GoTo<TokenGenViewModel>();
    }

    [RelayCommand]
    private void Logout()
    {
        _userRepository.DeleteUser();
        this.IsLoggedIn = false;
        _router.GoTo<HomeViewModel>();
    }

    private bool CanRegister()
    {
        return _employeeNumberRegex.IsMatch(EmployeeNumber) && _tokenGenerator.IsValid(AdminToken);
    }
}