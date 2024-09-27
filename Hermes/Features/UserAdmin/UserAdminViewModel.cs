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
using Hermes.Services;
using SukiUI.Dialogs;

namespace Hermes.Features.UserAdmin;

public partial class UserAdminViewModel : PageBase
{
    public ObservableCollection<User> Users { get; set; } = [];
    private readonly ISfcRepository _sfcRepository;
    private readonly Session _session;
    [ObservableProperty] private bool _canExportToCsv;
    [ObservableProperty] private bool _isDataLoading;
    [ObservableProperty] private string _searchEmployeeId = "";
    [ObservableProperty] private User _selectedUser = User.Null;
    private readonly ILogger _logger;
    private readonly ISukiDialogManager _dialogManager;
    private ManageUserDialogViewModel _manageUserDialogViewModel;
    private readonly FileService _fileService;

    public UserAdminViewModel(
        ISfcRepository sfcRepository,
        Session session,
        ISukiDialogManager dialogManager,
        FileService fileService,
        ILogger logger)
        : base(
            "User Admin",
            MaterialIconKind.Users,
            FeatureType.UserAdmin,
            1,
            [StationType.Labeling]
        )
    {
        _sfcRepository = sfcRepository;
        _session = session;
        _session.UserChanged += UserChanged;
        this._dialogManager = dialogManager;
        _fileService = fileService;
        _logger = logger;
    }

    private void UserChanged(User user)
    {
        this.Users.Clear();
    }

    [RelayCommand]
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
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage(Resources.txt_error, e.Message, NotificationType.Error));
            _logger.Error(e, e.Message);
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
            return await _sfcRepository.FindAllUsers(department, _session.UserLevel);
        }

        return await _sfcRepository.FindUserById(SearchEmployeeId, department, _session.UserLevel);
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

    private async Task Update(User user)
    {
        try
        {
            this._manageUserDialogViewModel.IsLoading = true;
            var affectedRows = await _sfcRepository.UpdateUser(user);
            if (affectedRows == 0)
            {
                throw new Exception(Resources.msg_user_not_found);
            }

            Messenger.Send(new ShowToastMessage(Resources.txt_success, Resources.msg_user_updated,
                NotificationType.Success));
            this._manageUserDialogViewModel.CloseDialog();
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage(Resources.txt_error, e.Message, NotificationType.Error));
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

    private async Task Add(User user)
    {
        try
        {
            this._manageUserDialogViewModel.IsLoading = true;
            await _sfcRepository.AddUser(user);
            Messenger.Send(new ShowToastMessage(Resources.txt_success, Resources.msg_user_added,
                NotificationType.Success));
            this._manageUserDialogViewModel.CloseDialog();
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage(Resources.txt_error, e.Message, NotificationType.Error));
        }
        finally
        {
            await this.FindAllUsers();
            this._manageUserDialogViewModel.IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveUser()
    {
        _dialogManager.CreateDialog()
            .WithTitle(Resources.txt_remove_user)
            .WithContent(Resources.msg_remove_user)
            .WithActionButton(Resources.txt_yes, _ => Dispatcher.UIThread.InvokeAsync(() => Remove(this.SelectedUser)),
                true)
            .WithActionButton(Resources.txt_no, dialog => dialog.Dismiss(), true)
            .TryShow();
    }

    private async Task Remove(User user)
    {
        try
        {
            await _sfcRepository.DeleteUser(user);
            Messenger.Send(new ShowToastMessage(Resources.txt_success, Resources.msg_user_deleted,
                NotificationType.Success));
            await this.FindAllUsers();
        }
        catch (Exception e)
        {
            Messenger.Send(new ShowToastMessage(Resources.txt_error, e.Message, NotificationType.Error));
        }
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