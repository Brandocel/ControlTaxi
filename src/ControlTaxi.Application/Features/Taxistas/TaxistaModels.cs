namespace ControlTaxi.Application.Features.Taxistas;

/// <summary>Fila de taxista para la tabla del catálogo.</summary>
public sealed class TaxistaRowDto
{
    public int Id { get; init; }
    public string Clave { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Unidad { get; init; } = string.Empty;
    public string Placas { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public string EstatusTexto { get; init; } = string.Empty;
}

/// <summary>Datos para crear o actualizar un taxista (upsert por clave).</summary>
public sealed record GuardarTaxistaRequest(
    string Clave,
    string Nombre,
    string? Telefono,
    string? Unidad,
    string? Placas,
    bool Activo);
