using ControlTaxi.Domain.Enums;

namespace ControlTaxi.Application.Features.Relaciones;

/// <summary>Filtro de búsqueda de relaciones (texto + rango de fechas).</summary>
public sealed record RelacionesFiltro(string? Busqueda = null, DateTime? FechaInicio = null, DateTime? FechaFin = null);

/// <summary>Modelo de lectura de una fila de relación (para la grilla).</summary>
public sealed class RelacionRowDto
{
    public int Id { get; init; }
    public string FolioApp { get; init; } = string.Empty;
    public string FolioOperacion { get; init; } = string.Empty;
    public string FolioPos { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public string Hotel { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public string Nacionalidad { get; init; } = string.Empty;
    public int Pax { get; init; }
    public int? TaxistaId { get; init; }
    public string TaxistaNombre { get; init; } = string.Empty;
    public string TransporteTipo { get; init; } = string.Empty;
    public string Gafete { get; init; } = string.Empty;
    public decimal Venta { get; init; }
    public decimal Dejada { get; init; }
    public decimal DejadaPagada { get; init; }
    public decimal Comision { get; init; }
    public decimal Pago { get; init; }
    public EstatusRelacion Estatus { get; init; }
    public EstatusDejada EstatusDejada { get; init; }
    public string EstatusTexto { get; init; } = string.Empty;
    public string EstatusDejadaTexto { get; init; } = string.Empty;
    public bool PuedePagarDejada { get; init; }
    public string FechaPagoDejada { get; init; } = string.Empty;
    public string UsuarioPagoDejada { get; init; } = string.Empty;
    public string TicketPagoDejada { get; init; } = string.Empty;
}

/// <summary>Datos para crear o actualizar (ligar) una relación.</summary>
public sealed record GuardarRelacionRequest(
    string FolioApp,
    string? FolioOperacion,
    string? FolioPos,
    string? Gafete,
    int TaxistaId,
    string? TaxistaNombre,
    string? TransporteTipo,
    string? Observaciones,
    DateTime? Fecha = null);

/// <summary>Datos para cobrar una dejada.</summary>
public sealed record PagarDejadaRequest(int RelacionId, string? Ticket);

/// <summary>Opción de taxista para el autocompletado al ligar.</summary>
public sealed record TaxistaOption(int Id, string Clave, string Nombre, string Telefono, string Unidad, string Placas);
