using System.Linq.Expressions;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.Enums;
using ControlTaxi.Domain.SharedKernel;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Application.Features.Relaciones;

/// <summary>
/// Orquesta el módulo Relaciones ticket–taxista. Devuelve <see cref="Result"/>
/// explícito y registra cada operación. Toda la regla de negocio vive en la
/// entidad <see cref="RelacionTicketTaxista"/>; este servicio solo coordina.
/// </summary>
public sealed class RelacionesService : IRelacionesService
{
    private const int MaxFilas = 300;

    private readonly IUnitOfWork _uow;
    private readonly IValidator<GuardarRelacionRequest> _validator;
    private readonly ILogger<RelacionesService> _logger;

    public RelacionesService(
        IUnitOfWork uow,
        IValidator<GuardarRelacionRequest> validator,
        ILogger<RelacionesService> logger)
    {
        _uow = uow;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<RelacionRowDto>>> BuscarAsync(RelacionesFiltro filtro, CancellationToken ct = default)
    {
        var q = string.IsNullOrWhiteSpace(filtro.Busqueda) ? null : filtro.Busqueda.Trim();
        var inicio = filtro.FechaInicio?.Date;
        var fin = filtro.FechaFin?.Date;

        // Sin filtro alguno: mostrar las de hoy (mismo comportamiento que el sistema anterior).
        if (q is null && inicio is null && fin is null)
            inicio = fin = DateTime.Today;

        var finExclusivo = fin?.AddDays(1);

        Expression<Func<RelacionTicketTaxista, bool>> predicado = r =>
            (inicio == null || r.Fecha >= inicio) &&
            (finExclusivo == null || r.Fecha < finExclusivo) &&
            (q == null ||
                r.FolioApp.Contains(q) ||
                r.FolioOperacion.Contains(q) ||
                r.FolioPos.Contains(q) ||
                r.TaxistaNombre.Contains(q) ||
                r.Gafete.Contains(q) ||
                r.Hotel.Contains(q) ||
                r.Destino.Contains(q));

        var relaciones = await _uow.Repository<RelacionTicketTaxista>().ListAsync(predicado, ct);

        var filas = relaciones
            .OrderByDescending(r => r.Fecha)
            .ThenByDescending(r => r.Id)
            .Take(MaxFilas)
            .Select(Map)
            .ToList();

        return Result.Success<IReadOnlyList<RelacionRowDto>>(filas);
    }

    public async Task<Result<int>> GuardarAsync(GuardarRelacionRequest request, string usuario, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<int>(Error.Validation(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));

        var folioApp = request.FolioApp.Trim();
        var folioOperacion = request.FolioOperacion?.Trim() ?? string.Empty;
        var repo = _uow.Repository<RelacionTicketTaxista>();

        // Upsert por FolioApp (o por FolioOperacion si ya existe ligado a ese ticket).
        var relacion = await repo.FirstOrDefaultAsync(
            r => r.FolioApp == folioApp ||
                 (folioOperacion != "" && r.FolioOperacion == folioOperacion), ct);

        var esNueva = relacion is null;
        relacion ??= new RelacionTicketTaxista(folioApp, request.Fecha?.Date ?? DateTime.Today);

        if (!string.IsNullOrWhiteSpace(folioOperacion))
            relacion.AsignarTicketPos(folioOperacion, request.FolioPos);

        if (request.TaxistaId > 0)
            relacion.LigarTaxista(request.TaxistaId, request.TaxistaNombre ?? string.Empty,
                request.TransporteTipo, request.Gafete, request.Observaciones);
        else
            relacion.ActualizarDescriptivos(request.TransporteTipo, request.Gafete, request.Observaciones);

        if (esNueva)
            await repo.AddAsync(relacion, ct);
        else
            repo.Update(relacion);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Relación {Estado} para FolioApp {FolioApp} (ticket POS {FolioOperacion}, taxista {TaxistaId}) por {Usuario}.",
            esNueva ? "creada" : "actualizada", folioApp, folioOperacion, request.TaxistaId, usuario);

        return Result.Success(relacion.Id);
    }

    public async Task<Result> PagarDejadaAsync(PagarDejadaRequest request, string usuario, CancellationToken ct = default)
    {
        var repo = _uow.Repository<RelacionTicketTaxista>();
        var relacion = await repo.GetByIdAsync(request.RelacionId, ct);
        if (relacion is null)
            return Result.Failure(Error.NotFound("No se encontró la relación a cobrar."));

        if (!relacion.PuedePagarDejada)
            return Result.Failure(Error.Conflict("La dejada no tiene importe pendiente o ya fue pagada."));

        relacion.PagarDejada(usuario, request.Ticket);
        repo.Update(relacion);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Dejada cobrada ({Importe:C}) para FolioApp {FolioApp} por {Usuario}, ticket {Ticket}.",
            relacion.DejadaPagada, relacion.FolioApp, usuario, request.Ticket);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<TaxistaOption>>> BuscarTaxistasAsync(string? query, int limite = 20, CancellationToken ct = default)
    {
        var q = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        var taxistas = await _uow.Repository<Taxista>().ListAsync(
            t => t.Estatus == EstatusGeneral.Activo &&
                 (q == null || t.Nombre.Contains(q) || t.Clave.Contains(q) || t.Unidad.Contains(q) || t.Placas.Contains(q)), ct);

        var opciones = taxistas
            .OrderBy(t => t.Nombre)
            .Take(limite)
            .Select(t => new TaxistaOption(t.Id, t.Clave, t.Nombre, t.Telefono, t.Unidad, t.Placas))
            .ToList();

        return Result.Success<IReadOnlyList<TaxistaOption>>(opciones);
    }

    private static RelacionRowDto Map(RelacionTicketTaxista r) => new()
    {
        Id = r.Id,
        FolioApp = r.FolioApp,
        FolioOperacion = r.FolioOperacion,
        FolioPos = r.FolioPos,
        Fecha = r.Fecha,
        Hotel = r.Hotel,
        Destino = r.Destino,
        Nacionalidad = r.Nacionalidad,
        Pax = r.Pax,
        TaxistaId = r.TaxistaId,
        TaxistaNombre = r.TaxistaNombre,
        TransporteTipo = r.TransporteTipo,
        Gafete = r.Gafete,
        Venta = r.Venta,
        Dejada = r.Dejada,
        DejadaPagada = r.DejadaPagada,
        Comision = r.Comision,
        Pago = r.Pago,
        Estatus = r.Estatus,
        EstatusDejada = r.EstatusDejada,
        EstatusTexto = TextoEstatus(r.Estatus),
        EstatusDejadaTexto = TextoEstatusDejada(r.EstatusDejada),
        PuedePagarDejada = r.PuedePagarDejada,
        FechaPagoDejada = r.FechaPagoDejada?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
        UsuarioPagoDejada = r.UsuarioPagoDejada,
        TicketPagoDejada = r.TicketPagoDejada
    };

    private static string TextoEstatus(EstatusRelacion estatus) => estatus switch
    {
        EstatusRelacion.PendienteDeLigar => "Pendiente de ligar",
        EstatusRelacion.PendienteDeTicket => "Pendiente de ticket",
        EstatusRelacion.Ligado => "Ligado",
        EstatusRelacion.ComisionPendiente => "Comisión pendiente",
        EstatusRelacion.ComisionPagada => "Comisión pagada",
        _ => estatus.ToString()
    };

    private static string TextoEstatusDejada(EstatusDejada estatus) => estatus switch
    {
        EstatusDejada.SinImporte => "Sin importe",
        EstatusDejada.Pendiente => "Pendiente",
        EstatusDejada.Pagado => "Pagado",
        _ => estatus.ToString()
    };
}
