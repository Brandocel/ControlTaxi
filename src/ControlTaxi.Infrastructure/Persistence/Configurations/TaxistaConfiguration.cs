using ControlTaxi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlTaxi.Infrastructure.Persistence.Configurations;

public sealed class TaxistaConfiguration : IEntityTypeConfiguration<Taxista>
{
    public void Configure(EntityTypeBuilder<Taxista> builder)
    {
        builder.ToTable("Taxistas");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Clave).HasMaxLength(20).IsRequired();
        builder.HasIndex(t => t.Clave).IsUnique();

        builder.Property(t => t.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Telefono).HasMaxLength(30);
        builder.Property(t => t.Unidad).HasMaxLength(30);
        builder.Property(t => t.Placas).HasMaxLength(20);
        builder.Property(t => t.Estatus).HasConversion<int>();
    }
}
