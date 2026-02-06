using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class AsignacionTurnoConfiguration : IEntityTypeConfiguration<AsignacionTurno>
{
    public void Configure(EntityTypeBuilder<AsignacionTurno> builder)
    {
        builder.HasOne(a => a.Empleado)
            .WithMany(e => e.AsignacionesTurno)
            .HasForeignKey(a => a.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Turno)
            .WithMany(t => t.AsignacionesTurno)
            .HasForeignKey(a => a.TurnoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
