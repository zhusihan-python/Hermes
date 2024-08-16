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

    public static DepartmentType[] Departments => EnumExtensions.GetValues<DepartmentType>();

    public RegisterViewModel()
    {
        // TODO: Move to repository and add DI
        _tokenGenerator = new TokenGenerator();
        _userRepository = new UserRepository();
        var user = _userRepository.GetUser();
        if (!user.IsNull)
        {
            IsManager = user.IsManager;
            EmployeeNumber = user.Number;
            Department = user.Department;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task Register()
    {
#if !DEBUG
        if (!_tokenGenerator.TryDecode(AdminToken, (int)Department, DateOnly.FromDateTime(DateTime.Now),
                out var _)) return;
#endif
        var user = new User()
        {
            IsManager = IsManager,
            Number = EmployeeNumber,
            Department = Department
        };
        await _userRepository.SaveUser(user);
        Messenger.Send(new NavigateMessage(new TokenGenViewModel(user)));
    }

    [RelayCommand]
    private void Logout()
    {
        _userRepository.DeleteUser();
        this.IsLoggedIn = false;
        Messenger.Send(new NavigateMessage(new HomeViewModel()));
    }

    private bool CanRegister()
    {
        return _employeeNumberRegex.IsMatch(EmployeeNumber) && _tokenGenerator.IsValid(AdminToken);
    }
}