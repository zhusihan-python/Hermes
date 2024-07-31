using Hermes.Common.Validators;
using Hermes.Models;
using System.Threading.Tasks;

namespace Hermes.Services;

public class StopService
{
    private readonly CompoundStopValidator _stopValidator;

    public StopService(
        RuleThreeFiveTenValidator ruleThreeFiveTenValidator,
        MachineStopValidator machineStopValidator)
    {
        this._stopValidator = new CompoundStopValidator()
            .Add(ruleThreeFiveTenValidator)
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