using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.PrecioUnitario).HasPrecision(18, 2);
        builder.Property(v => v.Cantidad).HasPrecision(18, 4);
        builder.Property(v => v.Total).HasPrecision(18, 2);
        builder.HasIndex(v => v.Fecha);

        builder.HasMany(v => v.Detalles)
            .WithOne()
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(v => v.Detalles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
