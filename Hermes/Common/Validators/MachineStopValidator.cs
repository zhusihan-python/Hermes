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
            result = new Stop(StopType.Machine);
        }

        return Task.FromResult(result);
    }
}