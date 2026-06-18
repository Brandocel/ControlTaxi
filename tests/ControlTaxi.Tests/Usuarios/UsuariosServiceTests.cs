using ControlTaxi.Application.Features.Usuarios;
using ControlTaxi.Domain.Authorization;
using ControlTaxi.Domain.Enums;
using ControlTaxi.Infrastructure.Persistence;
using ControlTaxi.Infrastructure.Persistence.Repositories;
using ControlTaxi.Infrastructure.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ControlTaxi.Tests.Usuarios;

public class UsuariosServiceTests
{
    private static (UsuariosService service, ControlTaxiDbContext ctx, Pbkdf2PasswordHasher hasher) Build()
    {
        var options = new DbContextOptionsBuilder<ControlTaxiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new ControlTaxiDbContext(options);
        var uow = new UnitOfWork(ctx);
        var hasher = new Pbkdf2PasswordHasher();
        var service = new UsuariosService(uow, hasher, new GuardarUsuarioValidator(), NullLogger<UsuariosService>.Instance);
        return (service, ctx, hasher);
    }

    [Fact]
    public async Task Crear_usuario_nuevo_sin_password_falla()
    {
        var (service, _, _) = Build();
        var req = new GuardarUsuarioRequest("ana", "Ana", null, RolUsuario.Operador, true, new[] { Permisos.Relaciones });
        var result = await service.GuardarAsync(req, "admin");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Crear_usuario_nuevo_hashea_password_y_guarda_permisos()
    {
        var (service, ctx, hasher) = Build();
        var req = new GuardarUsuarioRequest("ana", "Ana", "secreta1", RolUsuario.Supervisor, true,
            new[] { Permisos.Relaciones, "PermisoInventado" });

        var result = await service.GuardarAsync(req, "admin");

        result.IsSuccess.Should().BeTrue();
        var ana = await ctx.Usuarios.SingleAsync();
        ana.Rol.Should().Be(RolUsuario.Supervisor);
        hasher.Verify("secreta1", ana.PasswordHash).Should().BeTrue();
        ana.Permisos.Should().ContainSingle().Which.Should().Be(Permisos.Relaciones); // el inventado se descarta
    }

    [Fact]
    public async Task Editar_sin_password_conserva_la_actual()
    {
        var (service, ctx, hasher) = Build();
        await service.GuardarAsync(new GuardarUsuarioRequest("ana", "Ana", "secreta1", RolUsuario.Operador, true, Array.Empty<string>()), "admin");

        await service.GuardarAsync(new GuardarUsuarioRequest("ana", "Ana María", null, RolUsuario.Supervisor, true, Array.Empty<string>()), "admin");

        ctx.Usuarios.Count().Should().Be(1);
        var ana = await ctx.Usuarios.SingleAsync();
        ana.Nombre.Should().Be("Ana María");
        ana.Rol.Should().Be(RolUsuario.Supervisor);
        hasher.Verify("secreta1", ana.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Password_corta_falla_validacion()
    {
        var (service, _, _) = Build();
        var req = new GuardarUsuarioRequest("ana", "Ana", "123", RolUsuario.Operador, true, Array.Empty<string>());
        var result = await service.GuardarAsync(req, "admin");
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    [Fact]
    public async Task Desactivar_usuario_via_guardar()
    {
        var (service, ctx, _) = Build();
        await service.GuardarAsync(new GuardarUsuarioRequest("ana", "Ana", "secreta1", RolUsuario.Operador, true, Array.Empty<string>()), "admin");
        await service.GuardarAsync(new GuardarUsuarioRequest("ana", "Ana", null, RolUsuario.Operador, false, Array.Empty<string>()), "admin");

        var ana = await ctx.Usuarios.SingleAsync();
        ana.EstaActivo.Should().BeFalse();
    }
}
