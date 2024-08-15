using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Services;
using Hermes.Cipher.Types;
using Hermes.TokenGen.Common.Messages;
using Hermes.TokenGen.Models;
using System.Threading.Tasks;
using System;

namespace Hermes.TokenGen.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isManager;
    [ObservableProperty] private bool _isLoggedIn;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _employeeNumber = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _adminToken = "";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private DepartmentType _department;

    public static DepartmentType[] Departments => EnumExtensions.GetValues<DepartmentType>();

    public RegisterViewModel()
    {
        this.IsLoggedIn = FileService.FileExists(App.ConfigFullpath);
        if (IsLoggedIn)
        {
            var user = FileService.ReadJsonEncrypted<User>(App.ConfigFullpath);
            if (user is not null)
            {
                IsManager = user.IsManager;
                EmployeeNumber = user.Number;
                Department = user.Department;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task Register()
    {
        var user = new User()
        {
            IsManager = IsManager,
            Number = EmployeeNumber,
            Department = Department
        };
        await FileService.WriteJsonEncryptedAsync(App.ConfigFullpath, user);
        Messenger.Send(new NavigateMessage(
            new TokenGenViewModel(user)));
    }

    [RelayCommand]
    private void Logout()
    {
        FileService.DeleteFile(App.ConfigFullpath);
        this.IsLoggedIn = false;
        Messenger.Send(new NavigateMessage(new HomeViewModel()));
    }

    private bool CanRegister()
    {
        return int.TryParse(EmployeeNumber, out _) && !string.IsNullOrEmpty(AdminToken);
    }
}