using Hermes.Cipher.Types;
using Hermes.Types;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Models;

[PrimaryKey(nameof(Permission), nameof(Department), nameof(Level))]
public class FeaturePermission
{
    public PermissionType Permission { get; set; }
    public DepartmentType Department { get; set; }
    public UserLevel Level { get; set; }

    public bool HasPermission(UserLevel userLevel)
    {
        return Level <= userLevel;
    }
}