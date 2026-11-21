using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class ParametroProductoConfiguration : IEntityTypeConfiguration<ParametroProducto>
{
    public void Configure(EntityTypeBuilder<ParametroProducto> builder)
    {
        builder.ToTable("ParametrosProducto");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Rendimiento).HasPrecision(18, 4);
        builder.Property(p => p.Precio).HasPrecision(18, 2);
        builder.HasIndex(p => new { p.GranjaId, p.Tipo }).IsUnique();

        // Queso: 10 L = 1 kg, 30 días. Yogurt: 1 L = 1 L, 15 días.
        builder.HasData(
            new { Id = 1, GranjaId = 1, Tipo = TipoProducto.Queso, Rendimiento = 10m, VidaUtilDias = 30, Precio = 28000m },
            new { Id = 2, GranjaId = 1, Tipo = TipoProducto.Yogurt, Rendimiento = 1m, VidaUtilDias = 15, Precio = 6000m });
    }
}
