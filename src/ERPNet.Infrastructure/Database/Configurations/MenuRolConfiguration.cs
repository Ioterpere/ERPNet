using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class MenuRolConfiguration : IEntityTypeConfiguration<MenuRol>
{
    public void Configure(EntityTypeBuilder<MenuRol> builder)
    {
        builder.HasKey(mr => new { mr.MenuId, mr.RolId });

        builder.HasOne(mr => mr.Menu)
            .WithMany(m => m.MenusRoles)
            .HasForeignKey(mr => mr.MenuId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Rol)
            .WithMany(r => r.MenusRoles)
            .HasForeignKey(mr => mr.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
