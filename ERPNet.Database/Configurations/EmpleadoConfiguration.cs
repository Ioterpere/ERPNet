using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.HasIndex(e => e.DNI).IsUnique();
        builder.Property(e => e.Nombre).HasMaxLength(100);
        builder.Property(e => e.Apellidos).HasMaxLength(200);
        builder.Property(e => e.DNI).HasMaxLength(20);

        builder.HasOne(e => e.Seccion)
            .WithMany(s => s.Empleados)
            .HasForeignKey(e => e.SeccionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Encargado)
            .WithMany(e => e.Subordinados)
            .HasForeignKey(e => e.EncargadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
