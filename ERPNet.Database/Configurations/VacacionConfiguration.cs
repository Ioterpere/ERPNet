using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class VacacionConfiguration : IEntityTypeConfiguration<Vacacion>
{
    public void Configure(EntityTypeBuilder<Vacacion> builder)
    {
        builder.Property(v => v.Observaciones).HasMaxLength(1000);

        builder.HasOne(v => v.Empleado)
            .WithMany(e => e.Vacaciones)
            .HasForeignKey(v => v.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.AprobadoPor)
            .WithMany()
            .HasForeignKey(v => v.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
