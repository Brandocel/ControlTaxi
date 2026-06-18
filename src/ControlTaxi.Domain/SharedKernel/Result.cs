namespace ControlTaxi.Domain.SharedKernel;

/// <summary>
/// Patrón Result: representa el resultado de una operación que puede fallar,
/// sin recurrir a excepciones para el flujo normal. Reemplaza el patrón
/// "devolver null y registrar warning" que ocultaba errores en el sistema anterior.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Un resultado exitoso no puede tener error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Un resultado fallido debe tener un error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

/// <summary>Result que transporta un valor cuando la operación es exitosa.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;

    /// <summary>Valor de la operación. Lanza si se accede en un resultado fallido (error de programación).</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    public static implicit operator Result<T>(T value) => Success(value);
}
