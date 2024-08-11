using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Repositories;

public class UserRepository
{
    public async Task<User> GetUser(string token, DepartmentType department)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return User.Null;
        }

        // TODO: Implement
        await Task.Delay(500);
        return this.GetDebugUser();
    }

    public User GetDebugUser()
    {
        return new User()
        {
            Id = 0,
            EmployeeId = 0,
            Name = "Debug",
            UpdateLevel = PermissionLevel.Administrator,
            ViewLevel = PermissionLevel.Administrator,
            CanExit = true
        };
    }
}