using Hermes.Common.Validators;
using Hermes.Models;
using System.Threading.Tasks;

namespace Hermes.Services;

public class StopService
{
    private readonly CompoundStopValidator _stopValidator;

    public StopService(
        RuleThreeFiveTenValidator ruleThreeFiveTenValidator,
        MachineStopValidator machineStopValidator,
        CriticalLocationStopValidator criticalLocationStopValidator)
    {
        this._stopValidator = new CompoundStopValidator()
            .Add(criticalLocationStopValidator)
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

    public async Task<Stop> Calculate(UnitUnderTest unitUnderTest)
    {
        var stop = await this._stopValidator.ValidateAsync(unitUnderTest);
        stop.SerialNumber = unitUnderTest.SerialNumber;
        return stop;
    }
}