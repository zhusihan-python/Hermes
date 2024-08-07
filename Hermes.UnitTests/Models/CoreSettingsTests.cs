using Hermes.Models;

namespace HermesTests.Models;

public class CoreSettingsTests
{
    [Fact]
    public void GetFirstCriticalDefectLocation_WithCommaSeparatedLocations_ReturnsFirst()
    {
        var coreSettings = new CoreSettings
        {
            CriticalLocations = "a,b,c"
        };
        Assert.Equal("a", coreSettings.GetFirstCriticalDefectLocation());
    }

    [Fact]
    public void GetFirstCriticalDefectLocation_SingleLocation_ReturnsSingleLocation()
    {
        var coreSettings = new CoreSettings
        {
            CriticalLocations = "a"
        };
        Assert.Equal("a", coreSettings.GetFirstCriticalDefectLocation());
    }
}