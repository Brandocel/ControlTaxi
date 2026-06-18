using ControlTaxi.Domain.Authorization;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace ControlTaxi.Tests.Usuarios;

public class UsuarioTests
{
    private static Usuario Operador() => new("juan", "Juan", "hash", RolUsuario.Operador);

    [Fact]
    public void Administrador_tiene_todos_los_permisos()
    {
        var admin = new Usuario("admin", "Admin", "hash", RolUsuario.Administrador);
        admin.TienePermiso(Permisos.Usuarios).Should().BeTrue();
        admin.TienePermiso(Permisos.Relaciones).Should().BeTrue();
        admin.TienePermiso("CualquierCosa").Should().BeTrue();
    }

    [Fact]
    public void Operador_solo_tiene_los_permisos_asignados()
    {
        var u = Operador();
        u.EstablecerPermisos(new[] { Permisos.Relaciones });

        u.TienePermiso(Permisos.Relaciones).Should().BeTrue();
        u.TienePermiso(Permisos.Usuarios).Should().BeFalse();
    }

    [Fact]
    public void EstablecerPermisos_quita_duplicados_y_vacios()
    {
        var u = Operador();
        u.EstablecerPermisos(new[] { Permisos.Relaciones, Permisos.Relaciones, "  ", Permisos.Gastos });
        u.Permisos.Should().HaveCount(2);
    }

    [Fact]
    public void Activar_y_desactivar_cambian_estatus()
    {
        var u = Operador();
        u.EstaActivo.Should().BeTrue();
        u.Desactivar();
        u.EstaActivo.Should().BeFalse();
        u.Activar();
        u.EstaActivo.Should().BeTrue();
    }
}
