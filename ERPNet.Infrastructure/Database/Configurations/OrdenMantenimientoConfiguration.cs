using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class OrdenMantenimientoConfiguration : IEntityTypeConfiguration<OrdenMantenimiento>
{
    public void Configure(EntityTypeBuilder<OrdenMantenimiento> builder)
    {
        builder.Property(o => o.Descripcion).HasMaxLength(2000);

        builder.HasOne(o => o.Maquinaria)
            .WithMany(m => m.OrdenesMantenimiento)
            .HasForeignKey(o => o.MaquinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.TipoMantenimiento)
            .WithMany(t => t.OrdenesMantenimiento)
            .HasForeignKey(o => o.TipoMantenimientoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.AsignadoA)
            .WithMany()
            .HasForeignKey(o => o.AsignadoAId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
