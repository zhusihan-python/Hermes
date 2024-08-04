using Hermes.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hermes.Common.Validators;

public class CompoundStopValidator : IStopValidator
{
    private readonly List<IStopValidator> _validators = [];

    public CompoundStopValidator Add(IStopValidator validator)
    {
        this._validators.Add(validator);
        return this;
    }

    public virtual async Task<Stop> ValidateAsync(UnitUnderTest unitUnderTest)
    {
        foreach (var validator in _validators)
        {
            var validation = await validator.ValidateAsync(unitUnderTest);
            if (!validation.IsNull)
            {
                return validation;
            }
        }

        return Stop.Null;
    }
}