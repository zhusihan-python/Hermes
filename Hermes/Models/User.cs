using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Types;

namespace Hermes.Models;

public class User
{
    public static readonly User Null = new NullUser();

    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public DepartmentType Department { get; set; }
    public PermissionLevel ViewLevel { get; set; } = PermissionLevel.Level1;
    public string ViewLevelText => ViewLevel.ToTranslatedString();
    public PermissionLevel UpdateLevel { get; set; } = PermissionLevel.Level1;
    public string UpdateLevelText => UpdateLevel.ToTranslatedString();
    public bool CanExit { get; set; }
    public bool IsNull => this == Null;

    public bool CanView(PermissionLevel permissionLevel)
    {
        return ViewLevel >= permissionLevel;
    }

    public bool CanUpdate(PermissionLevel permissionLevel)
    {
        return UpdateLevel >= permissionLevel;
    }
}

public class NullUser : User
{
    public NullUser()
    {
    }
}