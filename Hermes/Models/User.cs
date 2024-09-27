using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Types;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.Models;

public class User
{
    public static readonly User Null = new NullUser();

    public string EmployeeId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public DepartmentType Department { get; set; }
    public UserLevel Level { get; set; }
    public string LevelText => Level.ToTranslatedString();
    public List<FeaturePermission> Permissions { get; set; } = [];

    public bool IsNull => this == Null;
    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && 
                           !string.IsNullOrWhiteSpace(Password) && 
                           !string.IsNullOrWhiteSpace(LevelText) && 
                           !string.IsNullOrWhiteSpace(EmployeeId);

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
        EmployeeId = "Debug";
        Name = "Debug";
        Department = DepartmentType.Admin;
        Level = UserLevel.Manager;
    }

    public override bool HasPermission(FeatureType featureType) => true;
}