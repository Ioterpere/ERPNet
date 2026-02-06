using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class PermisoRolMenuConfiguration : IEntityTypeConfiguration<PermisoRolMenu>
{
    public void Configure(EntityTypeBuilder<PermisoRolMenu> builder)
    {
        builder.HasKey(p => new { p.RolId, p.MenuId });

        builder.HasOne(p => p.Rol)
            .WithMany(r => r.PermisosRolMenu)
            .HasForeignKey(p => p.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Menu)
            .WithMany(m => m.PermisosRolMenu)
            .HasForeignKey(p => p.MenuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
