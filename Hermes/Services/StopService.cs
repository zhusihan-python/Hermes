using Hermes.Common.Validators;
using Hermes.Models;
using System.Threading.Tasks;
using Hermes.Repositories;

namespace Hermes.Services;

public class StopService
{
    private readonly CompoundStopValidator _stopValidator;

    public StopService(
        ConsecutiveDefectsValidator consecutiveDefectsValidator,
        MachineStopValidator machineStopValidator)
    {
        this._stopValidator = new CompoundStopValidator()
            .Add(consecutiveDefectsValidator)
            .Add(machineStopValidator);
    }

    public void Start()
    {
        // TODO
    }

    public void Stop()
    {
        // TODO
    }

    public async Task<Stop> Calculate(SfcResponse sfcResponse)
    {
        return await this._stopValidator.ValidateAsync(sfcResponse);
    }
}