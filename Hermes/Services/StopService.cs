using System;
using Hermes.Models;
using System.Threading.Tasks;
using Hermes.Utils.Validators;

namespace Hermes.Services;

public class StopService
{
    private readonly CompoundStopValidator _stopValidator;

    public StopService()
    {
        this._stopValidator = new CompoundStopValidator()
            .Add(new MachineStopValidator());
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