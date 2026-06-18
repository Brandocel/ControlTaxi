using ControlTaxi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlTaxi.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreUsuario).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.NombreUsuario).IsUnique();

        builder.Property(u => u.Nombre).HasMaxLength(150);
        builder.Property(u => u.PasswordHash).HasMaxLength(400).IsRequired();
        builder.Property(u => u.Rol).HasConversion<int>();
        builder.Property(u => u.Estatus).HasConversion<int>();

        // Lista de permisos serializada como string separado por '|'.
        builder.Property<List<string>>("_permisos")
            .HasColumnName("Permisos")
            .HasConversion(
                v => string.Join('|', v),
                v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    c => c.Aggregate(0, (h, v) => HashCode.Combine(h, v.GetHashCode())),
                    c => c.ToList()));
    }
}
