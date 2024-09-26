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
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Common.Messages;
using Hermes.Language;
using Hermes.Common;

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

    public UserAdminViewModel(
        ISfcRepository sfcRepository,
        Session session,
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
            var users = (await FindUsers(_session.UserDepartmentType)).OrderBy(x => x.EmployeeId).ToList();
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
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void AddUser()
    {
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void RemoveUser(object? value)
    {
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand]
    private void UserSelected(User user)
    {
        this.SelectedUser = user;
    }

    [RelayCommand]
    private async Task ExportToCsv()
    {
        // TODO
        throw new NotImplementedException();
        // await _userRepository.SaveSubUsersToCsv(folder[0].Path.AbsolutePath, SubUsers.ToList());
    }
}