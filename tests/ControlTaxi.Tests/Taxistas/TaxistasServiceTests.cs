using ControlTaxi.Application.Features.Taxistas;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Infrastructure.Persistence;
using ControlTaxi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ControlTaxi.Tests.Taxistas;

public class TaxistasServiceTests
{
    private static (TaxistasService service, ControlTaxiDbContext ctx) Build()
    {
        var options = new DbContextOptionsBuilder<ControlTaxiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new ControlTaxiDbContext(options);
        var service = new TaxistasService(new UnitOfWork(ctx), new GuardarTaxistaValidator(), NullLogger<TaxistasService>.Instance);
        return (service, ctx);
    }

    [Fact]
    public async Task Crear_taxista_nuevo()
    {
        var (service, ctx) = Build();
        var result = await service.GuardarAsync(new GuardarTaxistaRequest("T1", "Juan", "999", "VAN", "ABC1", true), "admin");

        result.IsSuccess.Should().BeTrue();
        var t = await ctx.Taxistas.SingleAsync();
        t.Clave.Should().Be("T1");
        t.Nombre.Should().Be("Juan");
        t.EstaActivo.Should().BeTrue();
    }

    [Fact]
    public async Task Guardar_con_clave_existente_actualiza()
    {
        var (service, ctx) = Build();
        await service.GuardarAsync(new GuardarTaxistaRequest("T1", "Juan", null, null, null, true), "admin");
        await service.GuardarAsync(new GuardarTaxistaRequest("T1", "Juan Pérez", "555", "TAXI", "XYZ9", false), "admin");

        ctx.Taxistas.Count().Should().Be(1);
        var t = await ctx.Taxistas.SingleAsync();
        t.Nombre.Should().Be("Juan Pérez");
        t.EstaActivo.Should().BeFalse();
    }

    [Fact]
    public async Task Guardar_sin_clave_falla()
    {
        var (service, _) = Build();
        var result = await service.GuardarAsync(new GuardarTaxistaRequest("", "Juan", null, null, null, true), "admin");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    [Fact]
    public async Task Eliminar_taxista_sin_relaciones()
    {
        var (service, ctx) = Build();
        await service.GuardarAsync(new GuardarTaxistaRequest("T1", "Juan", null, null, null, true), "admin");
        var id = (await ctx.Taxistas.SingleAsync()).Id;

        var result = await service.EliminarAsync(id, "admin");

        result.IsSuccess.Should().BeTrue();
        ctx.Taxistas.Count().Should().Be(0);
    }

    [Fact]
    public async Task No_elimina_taxista_con_relaciones()
    {
        var (service, ctx) = Build();
        await service.GuardarAsync(new GuardarTaxistaRequest("T1", "Juan", null, null, null, true), "admin");
        var taxista = await ctx.Taxistas.SingleAsync();

        var rel = new RelacionTicketTaxista("APP-1", DateTime.Today);
        rel.LigarTaxista(taxista.Id, taxista.Nombre);
        ctx.RelacionesTicketTaxista.Add(rel);
        await ctx.SaveChangesAsync();

        var result = await service.EliminarAsync(taxista.Id, "admin");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
        ctx.Taxistas.Count().Should().Be(1);
    }

    [Fact]
    public async Task Buscar_filtra_por_nombre()
    {
        var (service, _) = Build();
        await service.GuardarAsync(new GuardarTaxistaRequest("T1", "Juan Pérez", null, null, null, true), "admin");
        await service.GuardarAsync(new GuardarTaxistaRequest("T2", "María López", null, null, null, true), "admin");

        var result = await service.BuscarAsync("María");

        result.Value.Should().ContainSingle(x => x.Nombre == "María López");
    }
}
