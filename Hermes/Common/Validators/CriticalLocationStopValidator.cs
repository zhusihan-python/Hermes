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

    public Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        Defect defect = sfcResponse.GetDefectByLocation(this._coreSettings.CriticalLocations);
        if (!defect.IsNull && !sfcResponse.IsFail)
        {
            return Task.FromResult(new Stop(StopType.Line, sfcResponse)
            {
                Defect = defect,
                Details = $"Defect in critical location {defect.Location}"
            });
        }

        return Task.FromResult(Stop.Null);
    }
}