using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class MachineStopValidator(Settings settings) : IStopValidator
{
    public Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var result = Stop.Null;
        if (unitUnderTest is { IsSfcFail: true } &&
            !unitUnderTest.SfcResponseContains(settings.AdditionalOkSfcResponse))
        {
            result = new Stop(StopType.Machine)
            {
                Message = unitUnderTest.SfcResponse?.Content,
                Details = unitUnderTest.SfcResponse?.Details ?? "Error en SFC"
            };
        }

        return Task.FromResult(result);
    }
}