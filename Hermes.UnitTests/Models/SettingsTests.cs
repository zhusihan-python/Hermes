using Hermes.Models;

namespace HermesTests.Models;

public class SettingsTests
{
    [Fact]
    public void GetFirstCriticalDefectLocation_WithCommaSeparatedLocations_ReturnsFirst()
    {
        var sut = new Settings
        {
            CriticalLocations = "a,b,c"
        };
        Assert.Equal("a", sut.GetFirstCriticalDefectLocation());
    }

    [Fact]
    public void GetFirstCriticalDefectLocation_SingleLocation_ReturnsSingleLocation()
    {
        var sut = new Settings
        {
            CriticalLocations = "a"
        };
        Assert.Equal("a", sut.GetFirstCriticalDefectLocation());
    }
}