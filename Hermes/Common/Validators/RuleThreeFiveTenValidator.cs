using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Common.Validators;

public class RuleThreeFiveTenValidator : CompoundStopValidator
{
    public RuleThreeFiveTenValidator(
        ConsecutiveDefectsValidator consecutiveDefectsValidator,
        SameDefectsWithin1HourValidator sameDefectsWithin1HourValidator,
        AnyDefectsWithin1HourValidator anyDefectsWithin1HourValidator)
    {
        this.Add(consecutiveDefectsValidator)
            .Add(sameDefectsWithin1HourValidator)
            .Add(anyDefectsWithin1HourValidator);
    }

    public override async Task<Stop> ValidateAsync(SfcResponse sfcResponse)
    {
        var stop = await base.ValidateAsync(sfcResponse);
        if (!stop.IsNull)
        {
            stop.Type = StopType.Line;
        }

        return stop;
    }
}