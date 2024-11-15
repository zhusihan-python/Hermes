using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Common;
using Hermes.Language;
using Hermes.Models;
using Hermes.Repositories;
using Hermes.Services;
using Material.Icons;
using R3;
using SukiUI.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Hermes.Features.UserAdmin;

public partial class UserAdminViewModel : PageBase
{
    public RangeObservableCollection<User> Users { get; set; } = [];
    private readonly UserRepositoryProxy _userRepositoryProxy;
    private readonly Session _session;
    [ObservableProperty] private bool _canExportToCsv;
    [ObservableProperty] private bool _isDataLoading;
    [ObservableProperty] private string _searchEmployeeId = "";
    [ObservableProperty] private User _selectedUser = User.Null;

    private ManageUserDialogViewModel _manageUserDialogViewModel = null!;
    private readonly FileService _fileService;
    private readonly ILogger _logger;
    private readonly ISukiDialogManager _dialogManager;

    public UserAdminViewModel(
        ILogger logger,
        Session session,
        ISukiDialogManager dialogManager,
        FileService fileService,
        UserRepositoryProxy userRepositoryProxy)
        : base(
            "User Admin",
            MaterialIconKind.Users,
            1)
    {
        this._logger = logger;
        this._userRepositoryProxy = userRepositoryProxy;
        this._session = session;
        this._dialogManager = dialogManager;
        this._fileService = fileService;
        this.IsActive = true;
    }

    protected override void SetupReactiveExtensions()
    {
        this._session
            .LoggedUser
            .Do(user => this.Users.Clear())
            .Subscribe()
            .AddTo(ref Disposables);
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
                this.ShowErrorToast(Resources.msg_no_users_found);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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
            return await _userRepositoryProxy.FindAll(department, _session.UserLevel);
        }

        return await _userRepositoryProxy.FindById(SearchEmployeeId, department, _session.UserLevel);
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
            var affectedRows = await _userRepositoryProxy.UpdateUser(user);
            if (affectedRows == 0)
            {
                throw new Exception(Resources.msg_user_not_found);
            }

            this.ShowSuccessToast(Resources.msg_user_updated);
            this._manageUserDialogViewModel.CloseDialog();
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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
            await _userRepositoryProxy.Add(user);
            this.ShowSuccessToast(Resources.msg_user_added);
            this._manageUserDialogViewModel.CloseDialog();
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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

    private async Task Remove(User user)
    {
        try
        {
            _userRepositoryProxy.Delete(user);
            this.ShowSuccessToast(Resources.msg_user_deleted);
            await this.FindAllUsers();
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
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
        try
        {
            var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)
                    Application.Current?.ApplicationLifetime!)
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
            this.ShowSuccessToast(Resources.msg_users_exported_to_csv);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            this.ShowErrorToast(e.Message);
        }
    }
}