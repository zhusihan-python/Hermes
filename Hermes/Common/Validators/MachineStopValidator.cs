using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class MachineStopValidator : IStopValidator
{
    public Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var result = Stop.Null;
        if (unitUnderTest.IsSfcFail)
        {
            result = new Stop(StopType.Machine)
            {
                Details = unitUnderTest.SfcResponse?.Details ?? "Error en SFC"
            };
        }

        return Task.FromResult(result);
    }
}