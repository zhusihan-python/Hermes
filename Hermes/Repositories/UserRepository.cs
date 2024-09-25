using System;
using System.Threading.Tasks;
using Hermes.Cipher;
using Hermes.Cipher.Types;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Repositories;

public class UserRepository
{
    private readonly TokenGenerator _tokenGenerator;

    public UserRepository(TokenGenerator tokenGenerator)
    {
        _tokenGenerator = tokenGenerator;
    }

    public User GetUser(string token, DepartmentType department)
    {
        if (!_tokenGenerator.TryDecode(token.ToUpper(), department, DateOnly.FromDateTime(DateTime.Now), out int id))
        {
            return User.Null;
        }

        return new User()
        {
            Id = id,
            EmployeeId = id,
            Name = id.ToString(),
            Department = department,
            UpdateLevel = this.GetUpdateLevel(department),
            ViewLevel = this.GetViewLevel(department),
            CanExit = this.CanExit(department)
        };
    }

    private bool CanExit(DepartmentType department)
    {
        switch (department)
        {
            case DepartmentType.Admin:
            case DepartmentType.Aoi:
                return true;
            default:
                return false;
        }
    }

    private PermissionLevel GetViewLevel(DepartmentType department)
    {
        switch (department)
        {
            case DepartmentType.Admin:
                return PermissionLevel.Administrator;
            case DepartmentType.Auto:
            case DepartmentType.Aoi:
                return PermissionLevel.Level5;
            case DepartmentType.Qa:
                return PermissionLevel.Level3;
            default:
                return PermissionLevel.Level1;
        }
    }

    private PermissionLevel GetUpdateLevel(DepartmentType department)
    {
        switch (department)
        {
            case DepartmentType.Admin:
                return PermissionLevel.Administrator;
            case DepartmentType.Auto:
            case DepartmentType.Aoi:
                return PermissionLevel.Level5;
            case DepartmentType.Qa:
                return PermissionLevel.Level3;
            default:
                return PermissionLevel.Level1;
        }
    }

    public User GetDebugUser()
    {
        return new User()
        {
            Id = 0,
            EmployeeId = 0,
            Name = "Debug",
            Department = DepartmentType.Admin,
            UpdateLevel = PermissionLevel.Administrator,
            ViewLevel = PermissionLevel.Administrator,
            CanExit = true
        };
    }
}