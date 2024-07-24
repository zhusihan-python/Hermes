namespace Hermes.Utils.Validators;

public interface IValidator<in TInValue, out TValidation>
{
    TValidation ValidateAsync(TInValue sfcResponse);
}