namespace Hermes.Models;

public class CoreSettings
{
    public string CriticalLocations { get; set; } = "U1";

    public string GetFirstCriticalDefectLocation()
    {
        return CriticalLocations.Split(',')[0];
    }
}