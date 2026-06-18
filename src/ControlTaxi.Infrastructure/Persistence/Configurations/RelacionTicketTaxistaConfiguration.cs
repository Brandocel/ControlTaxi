using ControlTaxi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlTaxi.Infrastructure.Persistence.Configurations;

public sealed class RelacionTicketTaxistaConfiguration : IEntityTypeConfiguration<RelacionTicketTaxista>
{
    public void Configure(EntityTypeBuilder<RelacionTicketTaxista> builder)
    {
        builder.ToTable("RelacionesTicketTaxista");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FolioApp).HasMaxLength(60).IsRequired();
        builder.HasIndex(r => r.FolioApp).IsUnique();

        builder.Property(r => r.FolioOperacion).HasMaxLength(60);
        builder.HasIndex(r => r.FolioOperacion);
        builder.Property(r => r.FolioPos).HasMaxLength(120);
        builder.Property(r => r.Hotel).HasMaxLength(150);
        builder.Property(r => r.Origen).HasMaxLength(150);
        builder.Property(r => r.Destino).HasMaxLength(150);
        builder.Property(r => r.Nacionalidad).HasMaxLength(80);
        builder.Property(r => r.TaxistaNombre).HasMaxLength(150);
        builder.HasIndex(r => r.TaxistaId);
        builder.Property(r => r.TransporteTipo).HasMaxLength(20);
        builder.Property(r => r.Gafete).HasMaxLength(300);
        builder.Property(r => r.Unidad).HasMaxLength(30);
        builder.Property(r => r.Placas).HasMaxLength(20);
        builder.Property(r => r.Telefono).HasMaxLength(30);
        builder.Property(r => r.Observaciones).HasMaxLength(300);
        builder.Property(r => r.UsuarioPagoDejada).HasMaxLength(50);
        builder.Property(r => r.TicketPagoDejada).HasMaxLength(120);

        builder.Property(r => r.Venta).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Dejada).HasColumnType("decimal(18,2)");
        builder.Property(r => r.DejadaPagada).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Comision).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Pago).HasColumnType("decimal(18,2)");

        // Propiedades calculadas en el dominio: no se persisten.
        builder.Ignore(r => r.Estatus);
        builder.Ignore(r => r.EstatusDejada);
        builder.Ignore(r => r.PuedePagarDejada);
    }
}
