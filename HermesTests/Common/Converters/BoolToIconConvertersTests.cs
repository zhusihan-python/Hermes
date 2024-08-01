using Hermes.Common.Converters;
using Material.Icons;

namespace HermesTests.Common.Converters;

public class BoolToIconConvertersTests
{
    [Fact]
    public void Convert_Called_Ok()
    {
        AssertTrueAndFalseIcons(BoolToIconConverters.Animation, MaterialIconKind.Pause, MaterialIconKind.Play);
        AssertTrueAndFalseIcons(BoolToIconConverters.WindowLock, MaterialIconKind.Unlocked, MaterialIconKind.Lock);
        AssertTrueAndFalseIcons(BoolToIconConverters.Visibility, MaterialIconKind.EyeClosed, MaterialIconKind.Eye);
        AssertTrueAndFalseIcons(BoolToIconConverters.Simple, MaterialIconKind.Close, MaterialIconKind.Ticket);
        AssertTrueAndFalseIcons(BoolToIconConverters.Connection, MaterialIconKind.AccessPoint,
            MaterialIconKind.AccessPointOff);
    }

    private static void AssertTrueAndFalseIcons(BoolToIconConverter converter, MaterialIconKind trueIcon,
        MaterialIconKind falseIcon)
    {
        Assert.Equal(trueIcon, converter.Convert(true, typeof(MaterialIconKind), null, null!));
        Assert.Equal(falseIcon, converter.Convert(false, typeof(MaterialIconKind), null, null!));
    }

    [Fact]
    public void Convert_ValueIsNull_ReturnsNull()
    {
        Assert.Null(BoolToIconConverters.Animation.Convert(string.Empty, typeof(MaterialIconKind), null, null!));
    }

    [Fact]
    public void ConvertBack_Called_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            BoolToIconConverters.Animation.ConvertBack(null, typeof(MaterialIconKind), null, null!));
    }
}