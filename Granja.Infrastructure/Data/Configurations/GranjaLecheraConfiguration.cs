using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class GranjaLecheraConfiguration : IEntityTypeConfiguration<GranjaLechera>
{
    public void Configure(EntityTypeBuilder<GranjaLechera> builder)
    {
        builder.ToTable("Granjas");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(g => g.LitrosPorVaca).HasPrecision(18, 4);
        builder.Ignore(g => g.ProduccionDiariaEsperada);

        builder.HasMany(g => g.Parametros)
            .WithOne()
            .HasForeignKey(p => p.GranjaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(g => g.Parametros).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Valores iniciales (editables luego con PUT /api/granja): n = 10, l = 15 L.
        builder.HasData(new { Id = 1, Nombre = "Granja Lechera", NumeroVacas = 10, LitrosPorVaca = 15m });
    }
}
