using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.SharedKernel;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Application.Features.Taxistas;

/// <summary>
/// Catálogo de taxistas (CRUD). Devuelve <see cref="Result"/> y registra.
/// La eliminación protege la integridad: no borra un taxista con relaciones.
/// </summary>
public sealed class TaxistasService : ITaxistasService
{
    private const int MaxFilas = 500;

    private readonly IUnitOfWork _uow;
    private readonly IValidator<GuardarTaxistaRequest> _validator;
    private readonly ILogger<TaxistasService> _logger;

    public TaxistasService(IUnitOfWork uow, IValidator<GuardarTaxistaRequest> validator, ILogger<TaxistasService> logger)
    {
        _uow = uow;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TaxistaRowDto>>> BuscarAsync(string? busqueda = null, CancellationToken ct = default)
    {
        var q = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();

        var taxistas = await _uow.Repository<Taxista>().ListAsync(
            t => q == null ||
                 t.Clave.Contains(q) || t.Nombre.Contains(q) ||
                 t.Unidad.Contains(q) || t.Placas.Contains(q) || t.Telefono.Contains(q), ct);

        var filas = taxistas
            .OrderBy(t => t.Nombre)
            .Take(MaxFilas)
            .Select(Map)
            .ToList();

        return Result.Success<IReadOnlyList<TaxistaRowDto>>(filas);
    }

    public async Task<Result<int>> GuardarAsync(GuardarTaxistaRequest request, string usuario, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<int>(Error.Validation(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));

        var clave = request.Clave.Trim();
        var repo = _uow.Repository<Taxista>();
        var taxista = await repo.FirstOrDefaultAsync(t => t.Clave == clave, ct);
        var esNuevo = taxista is null;

        if (esNuevo)
            taxista = new Taxista(clave, request.Nombre, request.Telefono, request.Unidad, request.Placas);
        else
            taxista!.Actualizar(request.Nombre, request.Telefono, request.Unidad, request.Placas);

        if (request.Activo) taxista!.Activar(); else taxista!.Desactivar();

        if (esNuevo)
            await repo.AddAsync(taxista, ct);
        else
            repo.Update(taxista);

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Taxista {Estado} {Clave} - {Nombre} por {Usuario}.",
            esNuevo ? "creado" : "actualizado", clave, request.Nombre, usuario);

        return Result.Success(taxista.Id);
    }

    public async Task<Result> EliminarAsync(int id, string usuario, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Taxista>();
        var taxista = await repo.GetByIdAsync(id, ct);
        if (taxista is null)
            return Result.Failure(Error.NotFound("No se encontró el taxista."));

        // Proteger integridad: no borrar si tiene relaciones ligadas.
        var tieneRelaciones = await _uow.Repository<RelacionTicketTaxista>().AnyAsync(r => r.TaxistaId == id, ct);
        if (tieneRelaciones)
            return Result.Failure(Error.Conflict("El taxista tiene relaciones registradas. Desactívalo en lugar de eliminarlo."));

        repo.Remove(taxista);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Taxista eliminado {Clave} - {Nombre} por {Usuario}.", taxista.Clave, taxista.Nombre, usuario);

        return Result.Success();
    }

    private static TaxistaRowDto Map(Taxista t) => new()
    {
        Id = t.Id,
        Clave = t.Clave,
        Nombre = t.Nombre,
        Telefono = t.Telefono,
        Unidad = t.Unidad,
        Placas = t.Placas,
        Activo = t.EstaActivo,
        EstatusTexto = t.EstaActivo ? "Activo" : "Inactivo"
    };
}
