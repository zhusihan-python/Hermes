using Hermes.Cipher.Types;
using Hermes.Common.Extensions;
using Hermes.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Hermes.Models;

public partial class User : ObservableValidator
{
    public static readonly User Null = new NullUser();

    [Key] public int Id { get; set; }
    [MaxLength(64)] public string EmployeeId { get; set; } = "";
    [MaxLength(250)] public string Name { get; set; } = "";
    [MaxLength(64)] [ObservableProperty] private string _password = "";
    public DepartmentType Department { get; set; }
    public UserLevel Level { get; set; }
    [NotMapped] public string LevelText => Level.ToTranslatedString();
    [NotMapped] public List<FeaturePermission> Permissions { get; set; } = [];

    [NotMapped] public bool IsNull => this == Null;

    [NotMapped]
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