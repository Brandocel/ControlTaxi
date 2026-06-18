using ControlTaxi.Domain.SharedKernel;

namespace ControlTaxi.Application.Features.Relaciones;

/// <summary>Casos de uso del módulo Relaciones ticket–taxista.</summary>
public interface IRelacionesService
{
    /// <summary>Busca relaciones según filtro (texto + fechas). Sin filtro = las de hoy.</summary>
    Task<Result<IReadOnlyList<RelacionRowDto>>> BuscarAsync(RelacionesFiltro filtro, CancellationToken ct = default);

    /// <summary>Crea o actualiza (liga taxista / asigna ticket) una relación. Devuelve su Id.</summary>
    Task<Result<int>> GuardarAsync(GuardarRelacionRequest request, string usuario, CancellationToken ct = default);

    /// <summary>Cobra la dejada de una relación.</summary>
    Task<Result> PagarDejadaAsync(PagarDejadaRequest request, string usuario, CancellationToken ct = default);

    /// <summary>Busca taxistas activos para el autocompletado al ligar.</summary>
    Task<Result<IReadOnlyList<TaxistaOption>>> BuscarTaxistasAsync(string? query, int limite = 20, CancellationToken ct = default);
}
