using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlTaxi.Infrastructure.Persistence;

/// <summary>
/// Permite a las herramientas de EF Core (dotnet ef migrations) crear el contexto
/// en tiempo de diseño sin arrancar la app. La cadena solo se usa para generar SQL.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ControlTaxiDbContext>
{
    public ControlTaxiDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ControlTaxiDbContext>()
            .UseSqlServer("Server=localhost\\SQLEXPRESS;Database=ControlTaxiDesktop;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        return new ControlTaxiDbContext(options);
    }
}
