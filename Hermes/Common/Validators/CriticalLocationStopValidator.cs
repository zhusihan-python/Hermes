using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class CriticalLocationStopValidator : IStopValidator
{
    private readonly string _criticalLocations;


    public CriticalLocationStopValidator(string criticalLocations)
    {
        this._criticalLocations = criticalLocations;
    }

    public Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        Defect defect = sfcResponse.GetDefectByLocation(this._criticalLocations);
        if (!defect.IsNull)
        {
            return Task.FromResult(new Stop(StopType.Line, sfcResponse)
            {
                Defect = defect
            });
        }

        return Task.FromResult(Stop.Null);
    }
}