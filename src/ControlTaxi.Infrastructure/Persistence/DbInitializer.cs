using ControlTaxi.Domain.Entities;
using ControlTaxi.Domain.Enums;
using ControlTaxi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Infrastructure.Persistence;

/// <summary>
/// Aplica migraciones pendientes y siembra datos mínimos (usuario admin)
/// al arrancar. Reemplaza el "ejecutar SQL suelto en el arranque" anterior.
/// </summary>
public sealed class DbInitializer
{
    private readonly ControlTaxiDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(ControlTaxiDbContext context, IPasswordHasher hasher, ILogger<DbInitializer> logger)
    {
        _context = context;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if ((await _context.Database.GetPendingMigrationsAsync(ct)).Any())
        {
            _logger.LogInformation("Aplicando migraciones pendientes...");
            await _context.Database.MigrateAsync(ct);
        }

        if (!await _context.Usuarios.AnyAsync(ct))
        {
            _logger.LogInformation("No hay usuarios; creando admin por defecto (admin / admin123).");
            var admin = new Usuario("admin", "Administrador", _hasher.Hash("admin123"), RolUsuario.Administrador);
            await _context.Usuarios.AddAsync(admin, ct);
            await _context.SaveChangesAsync(ct);
        }

        await SeedTaxistasYRelacionesAsync(ct);
    }

    private async Task SeedTaxistasYRelacionesAsync(CancellationToken ct)
    {
        if (await _context.Taxistas.AnyAsync(ct))
            return;

        _logger.LogInformation("Sembrando taxistas y relaciones de ejemplo.");

        var t1 = new Taxista("T001", "Juan Pérez", "9981112233", "U-12", "ABC-123");
        var t2 = new Taxista("T002", "María López", "9984445566", "U-08", "XYZ-987");
        var t3 = new Taxista("T003", "Pedro Sánchez", "9987778899", "U-21", "JKL-456");
        await _context.Taxistas.AddRangeAsync(new[] { t1, t2, t3 }, ct);
        await _context.SaveChangesAsync(ct);

        // Relación ligada con dejada pendiente.
        var r1 = new RelacionTicketTaxista("APP-1001", DateTime.Today);
        r1.EstablecerDatosViaje("Hotel Riviera", "Aeropuerto", "Zona Hotelera", "USA", 3, "U-12", "ABC-123", "9981112233");
        r1.AsignarTicketPos("5001", "POS-5001");
        r1.LigarTaxista(t1.Id, t1.Nombre, "TX", "G-100", "Viaje directo");
        r1.EstablecerImportes(venta: 850m, dejada: 200m, comision: 120m);

        // Relación sin ligar todavía (pendiente de ligar).
        var r2 = new RelacionTicketTaxista("APP-1002", DateTime.Today);
        r2.EstablecerDatosViaje("Hotel Maya", "Centro", "Aeropuerto", "MEX", 2);

        // Relación ligada, dejada ya pagada.
        var r3 = new RelacionTicketTaxista("APP-1003", DateTime.Today.AddDays(-1));
        r3.EstablecerDatosViaje("Hotel Sol", "Aeropuerto", "Hotel Sol", "CAN", 4, "U-08", "XYZ-987");
        r3.AsignarTicketPos("5002", "POS-5002");
        r3.LigarTaxista(t2.Id, t2.Nombre, "VAN", "G-101", null);
        r3.EstablecerImportes(venta: 1200m, dejada: 300m, comision: 180m);
        r3.PagarDejada("admin", "TCK-300");

        await _context.RelacionesTicketTaxista.AddRangeAsync(new[] { r1, r2, r3 }, ct);
        await _context.SaveChangesAsync(ct);
    }
}
