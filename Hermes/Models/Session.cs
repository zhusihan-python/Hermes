using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Types;
using Hermes.Repositories;
using Hermes.Types;
using Reactive.Bindings;

namespace Hermes.Models;

public partial class Session : ObservableObject
{
    public ReactiveProperty<UutProcessorState> UutProcessorCurrentState { get; } = new(UutProcessorState.Stopped);


    [ObservableProperty] private string _path = string.Empty;
    public Stop Stop { get; set; } = Stop.Null;

    private User User
    {
        get => _user;
        set
        {
            _user = value;
            this.UserChanged?.Invoke(value);
        }
    }

    public event Action<User>? UserChanged;

    private User _user = User.Null;

    public bool IsLoggedIn => !_user.IsNull;
    public DepartmentType UserDepartment => _user.Department;
    public UserLevel UserLevel => _user.Level;

    public void ResetStop()
    {
        Stop = Stop.Null;
    }

    public bool HasUserPermission(PermissionType permissionType)
    {
        return this.User.HasPermission(permissionType);
    }

    public void UpdateUser(User user)
    {
        this.User = user;
    }

    public bool CanUserExit()
    {
        return this.HasUserPermission(PermissionType.Exit);
    }

    public void Logout()
    {
        this.User = User.Null;
    }

    public UserLevel[] GetLevelsBelowLoggedUser()
    {
        return EnumExtensions.GetValues<UserLevel>()
            .Where(x => x < UserLevel)
            .ToArray();
    }
}