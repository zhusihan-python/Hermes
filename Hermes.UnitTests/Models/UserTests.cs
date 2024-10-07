using Hermes.Models;
using Hermes.Types;

namespace HermesTests.Models;

public class UserTests
{
    [Fact]
    public void HasPermission_UserHasPermission_ReturnsTrue()
    {
        const PermissionType featureType = PermissionType.OpenSfcSimulator;
        var sut = new User
        {
            Permissions = [new FeaturePermission() { Permission = featureType }]
        };
        Assert.True(sut.HasPermission(featureType));
    }

    [Fact]
    public void HasPermission_NotUserHasPermission_ReturnsFalse()
    {
        const PermissionType featureType = PermissionType.OpenSfcSimulator;
        var sut = new User();
        Assert.False(sut.HasPermission(featureType));
    }
}