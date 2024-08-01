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
}