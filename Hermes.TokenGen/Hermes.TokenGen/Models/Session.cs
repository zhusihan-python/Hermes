using Hermes.Cipher.Types;

namespace Hermes.TokenGen.Models;

public class Session
{
    public User User { get; set; } = User.Null;
    public bool IsUserLoggedIn => !User.IsNull;
    public bool IsUserManager => User.IsManager;
    public string UserNumber => User.Number;
    public DepartmentType UserDepartment => User.Department;
}