using ControlTaxi.Application.Features.Relaciones;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Infrastructure.Persistence;
using ControlTaxi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ControlTaxi.Tests.Relaciones;

public class RelacionesServiceTests
{
    private static (RelacionesService service, ControlTaxiDbContext ctx) Build()
    {
        var options = new DbContextOptionsBuilder<ControlTaxiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new ControlTaxiDbContext(options);
        var uow = new UnitOfWork(ctx);
        var service = new RelacionesService(uow, new GuardarRelacionValidator(), NullLogger<RelacionesService>.Instance);
        return (service, ctx);
    }

    [Fact]
    public async Task Guardar_crea_relacion_nueva_ligando_taxista()
    {
        var (service, ctx) = Build();
        var taxista = new Taxista("T1", "Juan");
        ctx.Taxistas.Add(taxista);
        await ctx.SaveChangesAsync();

        var req = new GuardarRelacionRequest("APP-9", "5001", "POS-5001", "G-1", taxista.Id, "Juan", "TX", "obs");
        var result = await service.GuardarAsync(req, "admin");

        result.IsSuccess.Should().BeTrue();
        var creada = await ctx.RelacionesTicketTaxista.SingleAsync();
        creada.FolioApp.Should().Be("APP-9");
        creada.TaxistaId.Should().Be(taxista.Id);
        creada.FolioOperacion.Should().Be("5001");
    }

    [Fact]
    public async Task Guardar_actualiza_relacion_existente_por_folioapp()
    {
        var (service, ctx) = Build();
        ctx.RelacionesTicketTaxista.Add(new RelacionTicketTaxista("APP-9", DateTime.Today));
        await ctx.SaveChangesAsync();

        var req = new GuardarRelacionRequest("APP-9", "7777", null, null, 3, "Pedro", "VAN", null);
        var result = await service.GuardarAsync(req, "admin");

        result.IsSuccess.Should().BeTrue();
        ctx.RelacionesTicketTaxista.Count().Should().Be(1);
        var r = await ctx.RelacionesTicketTaxista.SingleAsync();
        r.FolioOperacion.Should().Be("7777");
        r.TaxistaId.Should().Be(3);
    }

    [Fact]
    public async Task Guardar_sin_taxista_ni_ticket_falla_validacion()
    {
        var (service, _) = Build();
        var req = new GuardarRelacionRequest("APP-9", null, null, null, 0, null, null, null);
        var result = await service.GuardarAsync(req, "admin");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    [Fact]
    public async Task Pagar_dejada_actualiza_la_relacion()
    {
        var (service, ctx) = Build();
        var rel = new RelacionTicketTaxista("APP-9", DateTime.Today);
        rel.EstablecerImportes(800, 200, 0);
        ctx.RelacionesTicketTaxista.Add(rel);
        await ctx.SaveChangesAsync();

        var result = await service.PagarDejadaAsync(new PagarDejadaRequest(rel.Id, "TCK-1"), "admin");

        result.IsSuccess.Should().BeTrue();
        var actualizada = await ctx.RelacionesTicketTaxista.SingleAsync();
        actualizada.DejadaPagada.Should().Be(200);
        actualizada.PuedePagarDejada.Should().BeFalse();
    }

    [Fact]
    public async Task Buscar_filtra_por_texto()
    {
        var (service, ctx) = Build();
        ctx.RelacionesTicketTaxista.Add(new RelacionTicketTaxista("APP-AAA", DateTime.Today));
        ctx.RelacionesTicketTaxista.Add(new RelacionTicketTaxista("APP-BBB", DateTime.Today));
        await ctx.SaveChangesAsync();

        var result = await service.BuscarAsync(new RelacionesFiltro("AAA"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(x => x.FolioApp == "APP-AAA");
    }
}
