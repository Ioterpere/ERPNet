using ERPNet.Domain.Entities;
using ERPNet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class PermisoRolRecursoConfiguration : IEntityTypeConfiguration<PermisoRolRecurso>
{
    public void Configure(EntityTypeBuilder<PermisoRolRecurso> builder)
    {
        builder.HasKey(p => new { p.RolId, p.RecursoId });

        builder.Property(p => p.Alcance)
            .HasDefaultValue(Alcance.Propio);

        builder.HasOne(p => p.Rol)
            .WithMany(r => r.PermisosRolRecurso)
            .HasForeignKey(p => p.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Recurso)
            .WithMany(r => r.PermisosRolRecurso)
            .HasForeignKey(p => p.RecursoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
