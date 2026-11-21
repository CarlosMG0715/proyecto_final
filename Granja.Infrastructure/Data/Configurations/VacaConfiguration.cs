using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class VacaConfiguration : IEntityTypeConfiguration<Vaca>
{
    public void Configure(EntityTypeBuilder<Vaca> builder)
    {
        builder.ToTable("Vacas");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Nombre).IsRequired().HasMaxLength(100);

        builder.HasOne<GranjaLechera>()
            .WithMany()
            .HasForeignKey(v => v.GranjaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Las 10 vacas iniciales (n = 10).
        builder.HasData(
            Enumerable.Range(1, 10)
                .Select(i => (object)new { Id = i, GranjaId = 1, Nombre = $"Vaca {i:00}", Activa = true })
                .ToArray());
    }
}
