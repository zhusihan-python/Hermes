using Hermes.Cipher.Types;
using Hermes.Types;

namespace Hermes.Models;

public class FeaturePermission
{
    public FeatureType Feature { get; set; }
    public DepartmentType Department { get; set; }
    public PermissionLevel Level { get; set; }

    public bool HasPermission(PermissionLevel permissionLevel)
    {
        return Level <= permissionLevel;
    }
}