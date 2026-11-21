using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class LoteProductoConfiguration : IEntityTypeConfiguration<LoteProducto>
{
    public void Configure(EntityTypeBuilder<LoteProducto> builder)
    {
        builder.ToTable("Lotes");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.LitrosUsados).HasPrecision(18, 4);
        builder.Property(l => l.CantidadInicial).HasPrecision(18, 4);
        builder.Property(l => l.CantidadDisponible).HasPrecision(18, 4);
        builder.Property(l => l.CantidadDanada).HasPrecision(18, 4);
        builder.HasIndex(l => new { l.Tipo, l.FechaProduccion });
    }
}
