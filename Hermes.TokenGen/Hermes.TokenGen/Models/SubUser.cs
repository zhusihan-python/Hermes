using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Types;

namespace Hermes.TokenGen.Models;

public partial class SubUser : User
{
    public User Manager { get; init; }

    [ObservableProperty] private string _token = "";
    public override DepartmentType Department => Manager.Department;

    public SubUser(User manager)
    {
        Manager = manager;
    }
}