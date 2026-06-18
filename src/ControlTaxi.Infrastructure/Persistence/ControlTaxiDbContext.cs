using System.Reflection;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Domain.Common;
using ControlTaxi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlTaxi.Infrastructure.Persistence;

/// <summary>
/// Contexto EF Core. Aplica todas las configuraciones por reflexión y rellena
/// automáticamente los campos de auditoría en cada guardado.
/// </summary>
public sealed class ControlTaxiDbContext : DbContext
{
    private readonly ICurrentUser? _currentUser;

    public ControlTaxiDbContext(DbContextOptions<ControlTaxiDbContext> options, ICurrentUser? currentUser = null)
        : base(options) => _currentUser = currentUser;

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Taxista> Taxistas => Set<Taxista>();
    public DbSet<RelacionTicketTaxista> RelacionesTicketTaxista => Set<RelacionTicketTaxista>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.Now;
        var usuario = _currentUser?.NombreUsuario ?? "sistema";

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreadoEn = ahora;
                entry.Entity.CreadoPor = usuario;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModificadoEn = ahora;
                entry.Entity.ModificadoPor = usuario;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
