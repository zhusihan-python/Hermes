using Hermes.Cipher.Types;
using Hermes.Types;

namespace Hermes.Models;

public class FeaturePermission
{
    public FeatureType Feature { get; set; }
    public DepartmentType Department { get; set; }
    public UserLevel Level { get; set; }

    public bool HasPermission(UserLevel userLevel)
    {
        return Level <= userLevel;
    }
}