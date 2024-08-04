using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class CriticalLocationStopValidator : IStopValidator
{
    private readonly CoreSettings _coreSettings;

    public CriticalLocationStopValidator(CoreSettings coreSettings)
    {
        this._coreSettings = coreSettings;
    }

    public Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var defect = unitUnderTest.GetDefectByLocation(this._coreSettings.CriticalLocations);
        if (defect.IsNull || unitUnderTest.IsFail)
        {
            return Task.FromResult(Stop.Null);
        }

        var stop = new Stop(StopType.Line);
        stop.Defects.Add(defect);
        stop.Details = $"Defect in critical location {defect.Location}";
        return Task.FromResult(stop);
    }
}