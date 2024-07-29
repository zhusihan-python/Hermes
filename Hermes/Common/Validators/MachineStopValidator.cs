using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class MachineStopValidator : IStopValidator
{
    public Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        var result = Stop.Null;
        if (sfcResponse.IsFail)
        {
            result = new Stop(StopType.Machine, sfcResponse);
        }

        return Task.FromResult(result);
    }
}