using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Types;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Models;

public class User
{
    public static readonly User Null = new NullUser();

    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public DepartmentType Department { get; set; }
    public PermissionLevel Level { get; set; }
    public string LevelText => Level.ToTranslatedString();
    public List<FeaturePermission> Permissions { get; set; } = [];

    public bool IsNull => this == Null;

    public virtual bool HasPermission(FeatureType featureType)
    {
        return Permissions
            .FirstOrDefault(p => p.Feature == featureType)
            ?.HasPermission(Level) ?? false;
    }
}

public class NullUser : User
{
}

public class DebugUser : User
{
    public DebugUser()
    {
        EmployeeId = 0;
        Name = "Debug";
        Department = DepartmentType.Admin;
        Level = PermissionLevel.Administrator;
    }

    public override bool HasPermission(FeatureType featureType) => true;
}