using ControlTaxi.Application.Features.Auth;
using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.Enums;
using ControlTaxi.Infrastructure.Persistence;
using ControlTaxi.Infrastructure.Persistence.Repositories;
using ControlTaxi.Infrastructure.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ControlTaxi.Tests.Auth;

public class AuthServiceTests
{
    private static (AuthService service, Pbkdf2PasswordHasher hasher, ControlTaxiDbContext ctx) Build()
    {
        var options = new DbContextOptionsBuilder<ControlTaxiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new ControlTaxiDbContext(options);
        var uow = new UnitOfWork(ctx);
        var hasher = new Pbkdf2PasswordHasher();
        var service = new AuthService(uow, hasher, new LoginValidator(), NullLogger<AuthService>.Instance);
        return (service, hasher, ctx);
    }

    [Fact]
    public async Task Login_with_valid_credentials_succeeds()
    {
        var (service, hasher, ctx) = Build();
        ctx.Usuarios.Add(new Usuario("admin", "Admin", hasher.Hash("admin123"), RolUsuario.Administrador));
        await ctx.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginRequest("admin", "admin123"));

        result.IsSuccess.Should().BeTrue();
        result.Value.NombreUsuario.Should().Be("admin");
        result.Value.Rol.Should().Be(RolUsuario.Administrador);
    }

    [Fact]
    public async Task Login_with_wrong_password_fails_unauthorized()
    {
        var (service, hasher, ctx) = Build();
        ctx.Usuarios.Add(new Usuario("admin", "Admin", hasher.Hash("admin123"), RolUsuario.Administrador));
        await ctx.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginRequest("admin", "mala"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task Login_with_inactive_user_is_blocked()
    {
        var (service, hasher, ctx) = Build();
        var u = new Usuario("juan", "Juan", hasher.Hash("clave"), RolUsuario.Operador);
        u.Desactivar();
        ctx.Usuarios.Add(u);
        await ctx.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginRequest("juan", "clave"));

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("inactivo");
    }

    [Fact]
    public async Task Login_with_empty_fields_fails_validation()
    {
        var (service, _, _) = Build();

        var result = await service.LoginAsync(new LoginRequest("", ""));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
