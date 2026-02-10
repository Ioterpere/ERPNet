using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class IncidenciaMarcajeConfiguration : IEntityTypeConfiguration<IncidenciaMarcaje>
{
    public void Configure(EntityTypeBuilder<IncidenciaMarcaje> builder)
    {
        builder.Property(i => i.Observaciones).HasMaxLength(1000);

        builder.HasOne(i => i.Marcaje)
            .WithOne(m => m.Incidencia)
            .HasForeignKey<IncidenciaMarcaje>(i => i.MarcajeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ValidadaPor)
            .WithMany()
            .HasForeignKey(i => i.ValidadaPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
