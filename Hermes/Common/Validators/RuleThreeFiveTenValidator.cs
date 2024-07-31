namespace Hermes.Common.Validators;

public class RuleThreeFiveTenValidator : CompoundStopValidator
{
    public RuleThreeFiveTenValidator(ConsecutiveDefectsValidator consecutiveDefectsValidator)
    {
        this.Add(consecutiveDefectsValidator);
        // TODO: SameFailuresWithin1Hour
        // TODO: FailuresWithin1Hour
    }
}