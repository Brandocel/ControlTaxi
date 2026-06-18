using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ControlTaxi.Tests.Relaciones;

public class RelacionTicketTaxistaTests
{
    private static RelacionTicketTaxista Nueva() => new("APP-1", DateTime.Today);

    [Fact]
    public void Sin_taxista_esta_pendiente_de_ligar()
    {
        Nueva().Estatus.Should().Be(EstatusRelacion.PendienteDeLigar);
    }

    [Fact]
    public void Con_taxista_pero_sin_ticket_esta_pendiente_de_ticket()
    {
        var r = Nueva();
        r.LigarTaxista(5, "Juan");
        r.Estatus.Should().Be(EstatusRelacion.PendienteDeTicket);
    }

    [Fact]
    public void Ligado_con_ticket_y_sin_comision_queda_Ligado()
    {
        var r = Nueva();
        r.LigarTaxista(5, "Juan");
        r.AsignarTicketPos("5001");
        r.Estatus.Should().Be(EstatusRelacion.Ligado);
    }

    [Fact]
    public void Con_comision_sin_pago_es_ComisionPendiente()
    {
        var r = Nueva();
        r.LigarTaxista(5, "Juan");
        r.AsignarTicketPos("5001");
        r.EstablecerImportes(venta: 800, dejada: 100, comision: 120);
        r.Estatus.Should().Be(EstatusRelacion.ComisionPendiente);
    }

    [Fact]
    public void Con_comision_pagada_es_ComisionPagada()
    {
        var r = Nueva();
        r.LigarTaxista(5, "Juan");
        r.AsignarTicketPos("5001");
        r.EstablecerImportes(venta: 800, dejada: 100, comision: 120);
        r.RegistrarPagoComision(120);
        r.Estatus.Should().Be(EstatusRelacion.ComisionPagada);
    }

    [Fact]
    public void Estatus_dejada_refleja_importe_y_pago()
    {
        var r = Nueva();
        r.EstatusDejada.Should().Be(EstatusDejada.SinImporte);

        r.EstablecerImportes(venta: 800, dejada: 200, comision: 0);
        r.EstatusDejada.Should().Be(EstatusDejada.Pendiente);
        r.PuedePagarDejada.Should().BeTrue();
    }

    [Fact]
    public void Pagar_dejada_marca_pagado_y_guarda_datos()
    {
        var r = Nueva();
        r.EstablecerImportes(venta: 800, dejada: 200, comision: 0);

        r.PagarDejada("admin", "TCK-9");

        r.DejadaPagada.Should().Be(200);
        r.EstatusDejada.Should().Be(EstatusDejada.Pagado);
        r.PuedePagarDejada.Should().BeFalse();
        r.UsuarioPagoDejada.Should().Be("admin");
        r.TicketPagoDejada.Should().Be("TCK-9");
        r.FechaPagoDejada.Should().NotBeNull();
    }

    [Fact]
    public void Pagar_dejada_sin_importe_lanza()
    {
        var r = Nueva();
        var act = () => r.PagarDejada("admin", "x");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void No_se_puede_pagar_dejada_dos_veces()
    {
        var r = Nueva();
        r.EstablecerImportes(venta: 800, dejada: 200, comision: 0);
        r.PagarDejada("admin", "TCK-9");

        var act = () => r.PagarDejada("admin", "TCK-10");
        act.Should().Throw<InvalidOperationException>();
    }
}
