using Hermes.Models;
using Hermes.Types;
using System.Threading.Tasks;
using Hermes.Repositories;

namespace Hermes.Common.Validators;

public class MachineStopValidator(Session session) : IStopValidator
{
    public Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        var result = Stop.Null;
        if (unitUnderTest is { IsSfcFail: true } &&
            !unitUnderTest.SfcResponseContains(session.Settings.AdditionalOkSfcResponse))
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