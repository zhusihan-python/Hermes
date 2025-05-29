using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Extensions;
using Hermes.Cipher.Types;
using Hermes.Types;
using R3;
using System.Linq;

namespace Hermes.Models;

public partial class
    Session : ObservableObject
{
    public ReactiveProperty<StateType> UutProcessorState { get; } = new(StateType.Stopped);
    public ReactiveProperty<User> LoggedUser { get; } = new(User.Null);

    [ObservableProperty] private string _path = string.Empty;

    public DepartmentType UserDepartment => LoggedUser.Value.Department;
    public UserLevel UserLevel => LoggedUser.Value.Level;

    public void UpdateUser(User user)
    {
        this.LoggedUser.Value = user;
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