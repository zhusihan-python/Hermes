namespace Hermes.Common.Validators;

public interface IValidator<in TInValue, out TValidation>
{
    TValidation ValidateAsync(TInValue unitUnderTest);
}