using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class MaquinariaConfiguration : IEntityTypeConfiguration<Maquinaria>
{
    public void Configure(EntityTypeBuilder<Maquinaria> builder)
    {
        builder.HasIndex(m => m.Codigo).IsUnique();
        builder.Property(m => m.Nombre).HasMaxLength(200);
        builder.Property(m => m.Codigo).HasMaxLength(50);
        builder.Property(m => m.Ubicacion).HasMaxLength(200);

        builder.HasOne(m => m.Seccion)
            .WithMany(s => s.Maquinarias)
            .HasForeignKey(m => m.SeccionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
