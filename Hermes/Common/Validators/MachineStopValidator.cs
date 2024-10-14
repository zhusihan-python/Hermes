using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;
using Hermes.Repositories;

namespace Hermes.Common.Validators;

public class MachineStopValidator(ISettingsRepository settingsRepository) : IStopValidator
{
    public Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var result = Stop.Null;
        if (unitUnderTest is { IsSfcFail: true, SfcResponse: not null } &&
            !unitUnderTest.SfcResponse.Content.Contains(settingsRepository.Settings.AdditionalOkSfcResponse))
        {
            result = new Stop(StopType.Machine)
            {
                Details = unitUnderTest.SfcResponse?.Details ?? "Error en SFC"
            };
        }

        return Task.FromResult(result);
    }
}