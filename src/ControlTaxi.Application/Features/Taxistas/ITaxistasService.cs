using ControlTaxi.Domain.SharedKernel;

namespace ControlTaxi.Application.Features.Taxistas;

/// <summary>Casos de uso del catálogo de Taxistas.</summary>
public interface ITaxistasService
{
    /// <summary>Lista taxistas filtrando por texto (clave, nombre, unidad, placas, teléfono).</summary>
    Task<Result<IReadOnlyList<TaxistaRowDto>>> BuscarAsync(string? busqueda = null, CancellationToken ct = default);

    /// <summary>Crea o actualiza un taxista (upsert por clave). Devuelve su Id.</summary>
    Task<Result<int>> GuardarAsync(GuardarTaxistaRequest request, string usuario, CancellationToken ct = default);

    /// <summary>Elimina un taxista del catálogo. Falla si tiene relaciones registradas.</summary>
    Task<Result> EliminarAsync(int id, string usuario, CancellationToken ct = default);
}
