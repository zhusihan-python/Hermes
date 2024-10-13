using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Hermes.Cipher.Types;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Types;
using Material.Icons;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Common;
using Hermes.Common.Aspects;
using Hermes.Services;
using SukiUI.Dialogs;

namespace Hermes.Features.UserAdmin;

public partial class UserAdminViewModel : PageBase
{
    public ObservableCollection<User> Users { get; set; } = [];
    private readonly UserProxy _userProxy;
    private readonly Session _session;
    [ObservableProperty] private bool _canExportToCsv;
    [ObservableProperty] private bool _isDataLoading;
    [ObservableProperty] private string _searchEmployeeId = "";
    [ObservableProperty] private User _selectedUser = User.Null;
    private readonly ISukiDialogManager _dialogManager;
    private ManageUserDialogViewModel _manageUserDialogViewModel;
    private readonly FileService _fileService;

    public UserAdminViewModel(
        Session session,
        ISukiDialogManager dialogManager,
        FileService fileService,
        UserProxy userProxy)
        : base(
            "User Admin",
            MaterialIconKind.Users,
            PermissionType.OpenUserAdmin,
            1,
            [StationType.Labeling]
        )
    {
        _userProxy = userProxy;
        _session = session;
        _session.UserChanged += UserChanged;
        this._dialogManager = dialogManager;
        _fileService = fileService;
    }

    private void UserChanged(User user)
    {
        this.Users.Clear();
    }

    [RelayCommand]
    [CatchExceptionAndShowErrorToast]
    private async Task FindUsers()
    {
        try
        {
            IsDataLoading = true;
            this.Users.Clear();
            var users = (await FindUsers(_session.UserDepartment)).OrderBy(x => x.EmployeeId).ToList();
            this.Users.AddRange(users);
            this.CanExportToCsv = users.Count != 0;
            if (this.CanExportToCsv)
            {
                this.SearchEmployeeId = string.Empty;
                this.SelectedUser = User.Null;
            }
            else
            {
                Messenger.Send(new ShowToastMessage(Resources.txt_error, Resources.msg_no_users_found,
                    NotificationType.Error));
            }
        }
        finally
        {
            IsDataLoading = false;
        }
    }

    private async Task<IEnumerable<User>> FindUsers(DepartmentType department)
    {
        if (string.IsNullOrEmpty(SearchEmployeeId))
        {
            return await _userProxy.FindAll(department, _session.UserLevel);
        }

        return await _userProxy.FindById(SearchEmployeeId, department, _session.UserLevel);
    }


    [RelayCommand]
    private void EditUser()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog =>
            {
                this._manageUserDialogViewModel = new ManageUserDialogViewModel(dialog, _session, SelectedUser);
                this._manageUserDialogViewModel.Accepted += (user) =>
                    Task.Run(() => Dispatcher.UIThread.InvokeAsync(() => this.Update(user)));
                return this._manageUserDialogViewModel;
            })
            .TryShow();
    }

    [CatchExceptionAndShowErrorToast]
    private async Task Update(User user)
    {
        try
        {
            this._manageUserDialogViewModel.IsLoading = true;
            var affectedRows = await _userProxy.UpdateUser(user);
            if (affectedRows == 0)
            {
                throw new Exception(Resources.msg_user_not_found);
            }

            Messenger.Send(new ShowToastMessage(Resources.txt_success, Resources.msg_user_updated,
                NotificationType.Success));
            this._manageUserDialogViewModel.CloseDialog();
        }
        finally
        {
            await this.FindAllUsers();
            this._manageUserDialogViewModel.IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddUser()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog =>
            {
                this._manageUserDialogViewModel = new ManageUserDialogViewModel(dialog, _session, null);
                this._manageUserDialogViewModel.Accepted += (user) =>
                    Task.Run(() => Dispatcher.UIThread.InvokeAsync(() => this.Add(user)));
                return this._manageUserDialogViewModel;
            })
            .TryShow();
    }

    [CatchExceptionAndShowErrorToast]
    private async Task Add(User user)
    {
        try
        {
            this._manageUserDialogViewModel.IsLoading = true;
            await _userProxy.Add(user);
            Messenger.Send(new ShowToastMessage(Resources.txt_success, Resources.msg_user_added,
                NotificationType.Success));
            this._manageUserDialogViewModel.CloseDialog();
        }
        finally
        {
            await this.FindAllUsers();
            this._manageUserDialogViewModel.IsLoading = false;
        }
    }

    [RelayCommand]
    private void RemoveUser()
    {
        _dialogManager.CreateDialog()
            .WithTitle(Resources.txt_remove_user)
            .WithContent(Resources.msg_remove_user)
            .WithActionButton(Resources.txt_yes, _ => Dispatcher.UIThread.InvokeAsync(() => Remove(this.SelectedUser)),
                true)
            .WithActionButton(Resources.txt_no, dialog => dialog.Dismiss(), true)
            .TryShow();
    }

    [CatchExceptionAndShowErrorToast]
    private async Task Remove(User user)
    {
        _userProxy.Delete(user);
        Messenger.Send(new ShowToastMessage(Resources.txt_success, Resources.msg_user_deleted,
            NotificationType.Success));
        await this.FindAllUsers();
    }

    private async Task FindAllUsers()
    {
        this.SearchEmployeeId = "";
        await this.FindUsers();
    }

    [RelayCommand]
    private void UserSelected(User user)
    {
        this.SelectedUser = user;
    }

    [RelayCommand]
    [CatchExceptionAndShowErrorToast]
    private async Task ExportToCsv()
    {
        var topLevel =
            TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime)
                .MainWindow);

        var file = await topLevel?.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            SuggestedFileName = @$"hermes_users_{DateTime.Now:yyyy_MM_dd_mm_ss}.csv",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("CSV Files") { Patterns = ["*.csv"] }
            }
        })!;
        if (file is null) return;

        var csv = "Employee Id, Employee Name, Department, Password\n";
        csv += Users
            .Select(user => $"{user.EmployeeId},{user.Name},{user.Department},{user.Password}")
            .Aggregate((a, b) => $"{a}\n{b}");
        await _fileService.WriteAllTextAsync(file.Path.AbsolutePath, csv);
        Messenger.Send(new ShowToastMessage("Exported to CSV", "The users have been exported to a CSV file.",
            NotificationType.Success));
    }
}