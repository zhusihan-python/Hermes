using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Types;
using Hermes.Types;
using R3;
using System.Linq;

namespace Hermes.Models;

public partial class Session : ObservableObject
{
    public ReactiveProperty<StateType> UutProcessorState { get; } = new(StateType.Stopped);
    public ReactiveProperty<User> LoggedUser { get; } = new(User.Null);

    [ObservableProperty] private string _path = string.Empty;
    public Stop Stop { get; set; } = Stop.Null;

    public bool IsLoggedIn => !LoggedUser.Value.IsNull;
    public DepartmentType UserDepartment => LoggedUser.Value.Department;
    public UserLevel UserLevel => LoggedUser.Value.Level;

    public void ResetStop()
    {
        Stop = Stop.Null;
    }

    public bool HasUserPermission(PermissionType permissionType)
    {
        return this.LoggedUser.Value.HasPermission(permissionType);
    }

    public void UpdateUser(User user)
    {
        this.LoggedUser.Value = user;
    }

    public bool CanUserExit()
    {
        return this.HasUserPermission(PermissionType.Exit);
    }

    public void Logout()
    {
        this.LoggedUser.Value = User.Null;
    }

    public UserLevel[] GetLevelsBelowLoggedUser()
    {
        return EnumExtensions.GetValues<UserLevel>()
            .Where(x => x < UserLevel)
            .ToArray();
    }
}