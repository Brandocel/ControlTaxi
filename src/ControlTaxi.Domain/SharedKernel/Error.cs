namespace ControlTaxi.Domain.SharedKernel;

/// <summary>
/// Representa un error de dominio con un código estable y un mensaje legible.
/// Se usa con <see cref="Result"/> para evitar lanzar excepciones en flujos esperados.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string message) => new("NotFound", message);
    public static Error Validation(string message) => new("Validation", message);
    public static Error Conflict(string message) => new("Conflict", message);
    public static Error Unauthorized(string message) => new("Unauthorized", message);
    public static Error Failure(string message) => new("Failure", message);

    public override string ToString() => string.IsNullOrEmpty(Code) ? Message : $"[{Code}] {Message}";
}
