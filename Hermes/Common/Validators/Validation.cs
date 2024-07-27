namespace Hermes.Common.Validators;

public abstract class Validation<T>
{
    public T Result { get; init; }
    public string Hint { get; init; }
    public abstract bool IsError { get; }

    protected Validation(T result, string hint = "")
    {
        this.Result = result;
        this.Hint = hint;
    }

    public override string ToString()
    {
        return $"Result: {Result} Hint: {Hint}";
    }
}