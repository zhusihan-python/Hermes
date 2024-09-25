using Hermes.Models;
using Hermes.Types;

namespace HermesTests.Models;

public class UserTests
{
    [Fact]
    public void HasPermission_UserHasPermission_ReturnsTrue()
    {
        const FeatureType featureType = FeatureType.SfcSimulator;
        var sut = new User
        {
            Permissions = [new FeaturePermission() { Feature = featureType }]
        };
        Assert.True(sut.HasPermission(featureType));
    }

    [Fact]
    public void HasPermission_NotUserHasPermission_ReturnsFalse()
    {
        const FeatureType featureType = FeatureType.SfcSimulator;
        var sut = new User();
        Assert.False(sut.HasPermission(featureType));
    }
}