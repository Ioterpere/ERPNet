using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Database.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.Property(m => m.Nombre).HasMaxLength(200);
        builder.Property(m => m.Path).HasMaxLength(500);
        builder.Property(m => m.IconClass).HasMaxLength(100);
        builder.Property(m => m.CustomClass).HasMaxLength(100);

        builder.HasOne(m => m.MenuPadre)
            .WithMany(m => m.SubMenus)
            .HasForeignKey(m => m.MenuPadreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Recurso)
            .WithMany(r => r.Menus)
            .HasForeignKey(m => m.RecursoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
