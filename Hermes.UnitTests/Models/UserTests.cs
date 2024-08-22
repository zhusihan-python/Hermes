using Hermes.Models;
using Hermes.Types;

namespace HermesTests.Models;

public class UserTests
{
    [Fact]
    public void CanView_UserCanView_ReturnsTrue()
    {
        var sut = new User
        {
            ViewLevel = PermissionLevel.Administrator
        };
        Assert.True(sut.CanView(PermissionLevel.Level1));
        Assert.True(sut.CanView(PermissionLevel.Administrator));
    }

    [Fact]
    public void CanView_NotUserCanView_ReturnsFalse()
    {
        var sut = new User
        {
            ViewLevel = PermissionLevel.Level1
        };
        Assert.False(sut.CanView(PermissionLevel.Level5));
        Assert.False(sut.CanView(PermissionLevel.Administrator));
    }

    [Fact]
    public void CanUpdate_UserCanUpdate_ReturnsTrue()
    {
        var sut = new User
        {
            UpdateLevel = PermissionLevel.Administrator
        };
        Assert.True(sut.CanUpdate(PermissionLevel.Level1));
        Assert.True(sut.CanUpdate(PermissionLevel.Administrator));
    }

    [Fact]
    public void CanUpdate_NotUserCanUpdate_ReturnsFalse()
    {
        var sut = new User
        {
            UpdateLevel = PermissionLevel.Level1
        };
        Assert.False(sut.CanUpdate(PermissionLevel.Level5));
        Assert.False(sut.CanUpdate(PermissionLevel.Administrator));
    }
}