using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Infrastructure.Persistence;
using ControlTaxi.Infrastructure.Persistence.Repositories;
using ControlTaxi.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ControlTaxi.Infrastructure;

/// <summary>Registra los servicios de infraestructura (BD, repos, seguridad).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurado.");

        services.AddDbContext<ControlTaxiDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<DbInitializer>();

        return services;
    }
}
