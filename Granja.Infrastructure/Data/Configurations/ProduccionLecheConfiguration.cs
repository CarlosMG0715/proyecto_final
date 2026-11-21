using Granja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Granja.Infrastructure.Data.Configurations;

public class ProduccionLecheConfiguration : IEntityTypeConfiguration<ProduccionLeche>
{
    public void Configure(EntityTypeBuilder<ProduccionLeche> builder)
    {
        builder.ToTable("Producciones");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Litros).HasPrecision(18, 4);

        // RN-01: una sola producción por fecha y por vaca.
        builder.HasIndex(p => new { p.Fecha, p.VacaId }).IsUnique();

        builder.HasOne<Vaca>()
            .WithMany()
            .HasForeignKey(p => p.VacaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
