using ERPNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERPNet.Infrastructure.Database.Configurations;

public class RolUsuarioConfiguration : IEntityTypeConfiguration<RolUsuario>
{
    public void Configure(EntityTypeBuilder<RolUsuario> builder)
    {
        builder.HasKey(ru => new { ru.UsuarioId, ru.RolId });

        builder.HasOne(ru => ru.Usuario)
            .WithMany(u => u.RolesUsuarios)
            .HasForeignKey(ru => ru.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ru => ru.Rol)
            .WithMany(r => r.RolesUsuarios)
            .HasForeignKey(ru => ru.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
