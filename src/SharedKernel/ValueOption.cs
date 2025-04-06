namespace SharedKernel;

public readonly struct ValueOption<T> where T : struct
{
    private readonly T? _value;

    private ValueOption(T? value) => _value = value;

    public bool IsSome => _value is not null;

    public static ValueOption<T> Some(T value) => new(value);

    public static ValueOption<T> None() => new(null);

    public TOut Match<TOut>(Func<T, TOut> some, Func<TOut> none) =>
        _value is not null ? some(_value.Value) : none();

    public T ValueOr(Func<T> valueProvider) => _value ?? valueProvider();
}
