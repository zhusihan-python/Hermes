using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class CriticalLocationStopValidator : IStopValidator
{
    private readonly Settings _settings;

    public CriticalLocationStopValidator(Settings settings)
    {
        this._settings = settings;
    }

    public Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var defect = unitUnderTest.GetDefectByLocation(this._settings.CriticalLocations);
        if (defect.IsNull || unitUnderTest.IsSfcFail)
        {
            return Task.FromResult(Stop.Null);
        }

        var stop = new Stop(StopType.Line);
        stop.Defects.Add(defect);
        stop.Details = $"Defect in critical location {defect.Location}";
        return Task.FromResult(stop);
    }
}