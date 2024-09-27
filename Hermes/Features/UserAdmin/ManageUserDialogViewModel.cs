using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Types;
using Hermes.Common.Messages;
using Hermes.Models;
using Hermes.Types;
using SukiUI.Dialogs;

namespace Hermes.Features.UserAdmin;

public partial class ManageUserDialogViewModel : ViewModelBase
{
    public event Action<User>? Accepted;

    [ObservableProperty] private bool _isAddUser;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSessionUserAdmin;
    [ObservableProperty] private User _user = User.Null;
    private readonly ISukiDialog _dialog;
    public UserLevel[] Levels { get; }
    public DepartmentType[] Departments => Enum.GetValues<DepartmentType>();

    public ManageUserDialogViewModel(ISukiDialog dialog, Session session, User? user = null)
    {
        this._dialog = dialog;
        this.IsSessionUserAdmin = session.UserDepartment == DepartmentType.Admin;
        this.Levels = session.UserLevel == UserLevel.Manager
            ? Enum.GetValues<UserLevel>()
            : session.GetLevelsBelowLoggedUser();
        this.IsAddUser = user == null;
        if (IsAddUser)
        {
            this.User = new User()
            {
                Department = session.UserDepartment,
                Level = UserLevel.Operator
            };
        }
        else
        {
            this.User = user!;
        }
    }

    [RelayCommand]
    private void Accept()
    {
        if (!this.User.IsValid)
        {
            Messenger.Send(new ShowToastMessage("Error", "Invalid user information", NotificationType.Error));
            return;
        }

        this.Accepted?.Invoke(this.User);
    }


    [RelayCommand]
    public void CloseDialog()
    {
        Dispatcher.UIThread.InvokeAsync(() => _dialog.Dismiss());
    }
}