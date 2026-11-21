using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class VentaDetalleConfiguration : IEntityTypeConfiguration<VentaDetalle>
{
    public void Configure(EntityTypeBuilder<VentaDetalle> builder)
    {
        builder.ToTable("VentaDetalles");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Cantidad).HasPrecision(18, 4);

        builder.HasOne<LoteProducto>()
            .WithMany()
            .HasForeignKey(d => d.LoteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
